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
    AbilityRuntime abilities;
    
    // Wall climb state tracking
    int currentWallDir = 0; // -1 sol, +1 sağ, 0 yok

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        s     = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        fsm   = GetComponent<PlayerStateMachine>();
        abilities = GetComponent<AbilityRuntime>();
    }

    public void Tick(float dt)
    {
        if (abilities && !abilities.CanClimb)
            return;

        // WallClimb aktifken kontroller
        if (fsm.Current == PlayerStateMachine.LocoState.WallClimb)
        {
            // Wall climb sırasında yönü koru ve dikey hızı güncelle
            motor.RequestWallClimb(currentWallDir, input.MoveY * cfg.climbSpeed);

            // Çıkış koşulları kontrol et
            if (cfg.exitOnHorizontalMove && Mathf.Abs(input.MoveX) > cfg.horizontalMoveThreshold)
            {
                // A/D ile çıkış
                motor.StopWallClimbRestoreGravity();
                currentWallDir = 0;
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "WallClimb->HorizontalMove");
                return;
            }
            
            if (cfg.exitOnJump && input.JumpPressed)
            {
                // Zıplama ile çıkış
                motor.StopWallClimbRestoreGravity();
                
                // Zıplama hesaplama
                float angle = Mathf.Deg2Rad * cfg.jumpExitAngle;
                Vector2 outDir = new Vector2(-currentWallDir * Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                float impulse = cfg.jumpExitImpulse;
                
                // Variable jump height kontrolü
                if (cfg.allowVariableJumpExit && !input.JumpHeld)
                {
                    impulse *= cfg.variableJumpMultiplier;
                }
                
                motor.RequestWallJump(outDir * impulse);
                currentWallDir = 0;
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpRise, "WallClimb->Jump");
                return;
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
            
            fsm.RequestTransition(PlayerStateMachine.LocoState.WallClimb, "WallClimb");
        }
    }
}
