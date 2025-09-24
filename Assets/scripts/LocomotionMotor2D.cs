using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class LocomotionMotor2D : MonoBehaviour
{
    public MovementConfig movement;
    public JumpConfig jumpCfg;
    public WallConfig wallCfg;
    public ClimbConfig climbCfg;
    public WallClimbConfig wallClimbCfg;

    Rigidbody2D rb;
    Sensors2D sensors;
    PlayerStateMachine fsm;

    // Intent/state
    float desiredX;
    float jumpHoldTimerMs;
    bool  allowCutJump;
    bool  wallSlideRequested;
    int   wallDir; // -1 left, +1 right
    bool  climbing;
    float climbVSpeed;
    bool wallClimbing;
    int  wallClimbDir;
    float wallClimbV;

    public float velocityX => rb.linearVelocity.x;
    public float velocityY => rb.linearVelocity.y;
    public int facingSign { get; private set; } = 1;

    [Header("Facing")]
    [SerializeField] float faceVxDeadzone = 0.05f; // yatay hız bu eşiğin üstündeyse velocity'den yüz belirle

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sensors = GetComponent<Sensors2D>();
        fsm = GetComponent<PlayerStateMachine>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.gravityScale = movement ? movement.gravityScale : 3.5f;
    }

    // --------- Requests from controllers ----------
    public void RequestHorizontalIntent(float moveX)
    {
        desiredX = Mathf.Clamp(moveX, -1f, 1f);
        // facingSign'ı burada DEĞİŞTİRME.
    }

    public void RequestJump(float upVelocity, bool cutEnabled)
    {
        var v = rb.linearVelocity; v.y = upVelocity; rb.linearVelocity = v;
        jumpHoldTimerMs = jumpCfg ? jumpCfg.jumpHoldMs : 180f;
        allowCutJump = cutEnabled;
        fsm.TriggerPhase(PlayerStateMachine.PhaseState.None);
    }

    public void RequestWallSlide(int dir)
    {
        wallSlideRequested = true;
        wallDir = Mathf.Clamp(dir, -1, 1);
    }

    public void RequestWallJump(Vector2 impulse)
    {
        rb.linearVelocity = impulse;
        wallSlideRequested = false;
        fsm.TriggerPhase(PlayerStateMachine.PhaseState.WallJump);
    }


    public void RequestClimb(float verticalSpeed)
    {
        climbing = true;
        climbVSpeed = verticalSpeed;
        rb.gravityScale = 0f;
    }

    public void RequestStopClimbRestoreGravity()
    {
        climbing = false;
        rb.gravityScale = movement ? movement.gravityScale : 3.5f;
    }

    public void RequestWallClimb(int dir, float vSpeed)
    {
        wallClimbing = true;
        wallClimbDir = Mathf.Clamp(dir, -1, 1);
        wallClimbV   = vSpeed;
        rb.gravityScale = 0f;
        // yüzeye yapışmayı artırmak için hafif içe doğru çek
        var v = rb.linearVelocity; v.x = Mathf.Lerp(v.x, 0f, wallClimbCfg ? wallClimbCfg.stickFriction : 0.15f);
        rb.linearVelocity = v;
    }

    public void StopWallClimbRestoreGravity()
    {
        wallClimbing = false;
        rb.gravityScale = movement ? movement.gravityScale : 3.5f;
    }
    // ----------------------------------------------

    public void PhysicsStep(float fixedDt)
    {

        // Tırmanış
        if (climbing)
        {
            rb.linearVelocity = new Vector2(0f, climbVSpeed);
            return;
        }

        // Wall climb
        if (wallClimbing)
        {
            // yatay sabit, dikey kontrol: MoveY motor dışından setlenir (controller hesaplar)
            rb.linearVelocity = new Vector2(0f, wallClimbV);
            return;
        }

        // Yatay hız ayarı (smoothing)
        float target = desiredX * (movement ? movement.maxRunSpeed : 6f);
        float accelRate = (Mathf.Abs(target) > 0.01f)
            ? (1f / Mathf.Max(0.01f, movement ? movement.accelTime : 0.12f))
            : (1f / Mathf.Max(0.01f, movement ? movement.decelTime : 0.12f));
        float ctrl = sensors.isGrounded ? 1f : (movement ? movement.airControl : 0.5f);
        float maxStep = (movement ? movement.maxRunSpeed : 6f) * accelRate * ctrl * fixedDt;
        float vx = Mathf.MoveTowards(rb.linearVelocity.x, target, maxStep);

        // Variable jump hold / cut
        if (jumpHoldTimerMs > 0f)
            jumpHoldTimerMs -= fixedDt * 1000f;
        else if (allowCutJump && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y * (jumpCfg ? jumpCfg.cutMultiplier : 0.55f));
            allowCutJump = false;
        }
        if (sensors.headBlocked && rb.linearVelocity.y > 0f && (jumpCfg ? jumpCfg.ceilingCancel : true))
        {
            rb.linearVelocity = new Vector2(vx, 0f);
            allowCutJump = false;
            return;
        }

        // Wall slide düşüş hızı sınırı
        float vy = rb.linearVelocity.y;
        if (wallSlideRequested && !sensors.isGrounded)
        {
            vy = Mathf.Max(vy, -(movement ? movement.wallSlideMaxFall : 2.5f));
        }

        rb.linearVelocity = new Vector2(vx, vy);
        wallSlideRequested = false; // one-frame
        // wallClimbing burada TEMİZLENMEZ; kontrolcü kapatana kadar devam eder

        // Facing çözümü
        ResolveFacing();
    }

    void ResolveFacing()
    {
        float vx = rb.linearVelocity.x;

        // 1) Yeterli yatay hız varsa velocity yönüne bak
        if (Mathf.Abs(vx) >= faceVxDeadzone)
        {
            facingSign = vx > 0 ? 1 : -1;
            return;
        }

        // 2) State'e göre kestirim
        switch (fsm.Current)
        {
            case PlayerStateMachine.LocoState.WallSlide:
            case PlayerStateMachine.LocoState.WallClimb:
                if (sensors.wallRight) { facingSign = +1; return; }
                if (sensors.wallLeft)  { facingSign = -1; return; }
                break;

        }

        // 3) Aksi halde önceki facing'i koru (dikey hareketlerde titreme olmaz)
    }
}
