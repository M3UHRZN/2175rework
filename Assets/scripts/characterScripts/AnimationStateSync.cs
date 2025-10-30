using UnityEngine;

[DisallowMultipleComponent]
public class AnimationStateSync : MonoBehaviour
{
    public AnimationFacade anim;

    // <<< YENİ: Flip seçenekleri >>>
    [Header("Facing/Flip")]
    [Tooltip("Boş bırakırsan çocuklardaki SpriteRenderer aranır")]
    public SpriteRenderer spriteRenderer;
    [Tooltip("Sprite'ın orijinali sola bakıyorsa true yap")]
    public bool invertX = false;
    [Tooltip("flipX yerine ölçekle çevir (UI/shader uyumsuzluklarında işe yarar)")]
    public bool flipByScale = false;
    [Tooltip("flipByScale açıksa hangi transform ölçeklenecek? Boşsa bu obje")]
    public Transform flipRoot;

    PlayerStateMachine fsm;
    Sensors2D sensors;
    LocomotionMotor2D motor;

    Vector3 baseScale; // flipByScale için
    
    // Trigger reset tracking
    private bool pendingJumpReset;
    private bool pendingLandReset;
    private bool pendingWallJumpReset;

    void Awake()
    {
        fsm = GetComponent<PlayerStateMachine>();
        sensors = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        if (!anim) anim = GetComponent<AnimationFacade>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!flipRoot) flipRoot = transform;
        baseScale = flipRoot.localScale;

        if (fsm != null)
        {
            fsm.OnLocoChanged += (oldS, newS) =>
            {
                if (anim && anim.animator)
                {
                    // JumpRise'a GİRİŞTE trigger'ı setle
                    if (newS == PlayerStateMachine.LocoState.JumpRise)
                    {
                        anim.Trigger("Jump");
                        pendingJumpReset = true;
                    }
                    // JumpRise'tan ÇIKIŞTA reset'le
                    else if (oldS == PlayerStateMachine.LocoState.JumpRise && pendingJumpReset)
                    {
                        anim.ResetTrigger("Jump");
                        pendingJumpReset = false;
                    }
                    
                    // WallJump reset - JumpRise'tan ÇIKIŞTA reset'le (wall jump'tan sonra)
                    if (oldS == PlayerStateMachine.LocoState.JumpRise && pendingWallJumpReset)
                    {
                        anim.ResetTrigger("WallJump");
                        pendingWallJumpReset = false;
                    }
                    
                    // Land trigger - Idle/Run'a GİRİŞTE (just landed olduğunda)
                    if ((newS == PlayerStateMachine.LocoState.Idle || newS == PlayerStateMachine.LocoState.Run) && sensors.justLanded)
                    {
                        anim.Trigger("Land");
                        pendingLandReset = true;
                    }
                    // Land trigger reset - Idle/Run'tan ÇIKIŞTA
                    else if ((oldS == PlayerStateMachine.LocoState.Idle || oldS == PlayerStateMachine.LocoState.Run) && pendingLandReset)
                    {
                        anim.ResetTrigger("Land");
                        pendingLandReset = false;
                    }
                }
            };
            fsm.OnPhaseTriggered += (p) =>
            {
                if (anim && anim.animator)
                {
                    if (p == PlayerStateMachine.PhaseState.WallJump)
                    {
                        anim.Trigger("WallJump");
                        pendingWallJumpReset = true;
                    }
                }
            };
        }
    }

    public void LateSync()
    {
        // Animator paramları (sadece animator varsa)
        if (anim && anim.animator)
        {
            anim.SetBool("Grounded", sensors.isGrounded);
            anim.SetBool("WallSliding", fsm.Current == PlayerStateMachine.LocoState.WallSlide);
            anim.SetBool("WallClimbing", fsm.Current == PlayerStateMachine.LocoState.WallClimb);
            anim.SetBool("Climbing", fsm.Current == PlayerStateMachine.LocoState.Climb);
            anim.SetFloat("SpeedX", Mathf.Abs(motor.velocityX));
            anim.SetFloat("SpeedY", motor.velocityY);
            anim.SetInt("LocoState", (int)fsm.Current);
        }

        // --- FACING / FLIP (YENİ) ---
        int sign = motor.facingSign; // +1 sağ, -1 sol (velocity + state'e göre)
        bool faceLeft = sign < 0;
        if (flipByScale)
        {
            float sx = Mathf.Abs(baseScale.x) * ((invertX ? !faceLeft : faceLeft) ? -1f : 1f);
            var sc = flipRoot.localScale;
            sc.x = sx;
            flipRoot.localScale = sc;
        }
        else if (spriteRenderer)
        {
            // flipX sprite'ın orijinaline göre
            spriteRenderer.flipX = invertX ? !faceLeft : faceLeft;
        }
    }
}
