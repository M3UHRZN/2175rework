using UnityEngine;

[DisallowMultipleComponent]
public class WallClimbController : MonoBehaviour
{
    public WallClimbConfig cfg;
    public WallConfig wallCfg; // Wall jump için gerekli

    InputAdapter input;
    Sensors2D s;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;
    LedgeHangController ledgeController;
    
    // Wall climb state tracking
    int currentWallDir = 0; // -1 sol, +1 sağ, 0 yok

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        s     = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        fsm   = GetComponent<PlayerStateMachine>();
        ledgeController = GetComponent<LedgeHangController>();
    }

    public void Tick(float dt)
    {
        // WallClimb aktifken kontroller
        if (fsm.Current == PlayerStateMachine.LocoState.WallClimb)
        {
            // Wall climb sırasında yönü koru ve dikey hızı güncelle
            motor.RequestWallClimb(currentWallDir, input.MoveY * cfg.climbSpeed);

            // ayrılma koşulları
            if (!(s.onClimbableAny && (s.wallLeft || s.wallRight)))
            {
                motor.StopWallClimbRestoreGravity();
                currentWallDir = 0; // Reset
                
                // Exit transition'ları
                if (!s.isGrounded && (s.wallLeft || s.wallRight))
                    fsm.RequestTransition(PlayerStateMachine.LocoState.WallSlide, "ExitWallClimb->Slide");
                else
                    fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "ExitWallClimb");
                
                // Ledge detection pozisyonunu sıfırla
                ledgeController?.OnWallClimbExit();
            }
            else if (input.JumpPressed)
            {
                motor.StopWallClimbRestoreGravity();
                currentWallDir = 0; // Reset
                
                // Wall jump yap (duvardan ayrılarak)
                float a = Mathf.Deg2Rad * (wallCfg ? wallCfg.wallJumpAngle : 55f);
                Vector2 outDir = new Vector2(-currentWallDir * Mathf.Cos(a), Mathf.Sin(a)).normalized;
                motor.RequestWallJump(outDir * (wallCfg ? wallCfg.wallJumpImpulse : 10f));
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpRise, "WallClimbJump");
                
                // Ledge detection pozisyonunu sıfırla
                ledgeController?.OnWallClimbExit();
            }
            return; // Wall climb aktifken giriş kontrolü yapma
        }

        // Wall climb giriş kontrolü
        int dir = 0;
        bool want = Mathf.Abs(input.MoveY) > cfg.enterMinMoveY;
        if (s.wallLeft && s.onClimbableLeft && (want || input.MoveX < -0.1f))  dir = -1;
        if (s.wallRight && s.onClimbableRight && (want || input.MoveX >  0.1f)) dir = +1;

        if (dir != 0)
        {
            currentWallDir = dir; // Yönü kaydet
            float v = input.MoveY * cfg.climbSpeed; // yukarı/asağı
            motor.RequestWallClimb(dir, v);
            
            // Sadece daha yüksek öncelikli state'e geçiş yapabilir
            if (fsm.CanTransitionTo(PlayerStateMachine.LocoState.WallClimb))
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.WallClimb, "WallClimb");
            }
        }
    }
}
