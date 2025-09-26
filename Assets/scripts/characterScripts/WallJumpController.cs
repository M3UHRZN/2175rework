using UnityEngine;

[DisallowMultipleComponent]
public class WallJumpController : MonoBehaviour
{
    public WallJumpConfig wallJumpCfg;
    
    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        sensors = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        fsm = GetComponent<PlayerStateMachine>();
    }

    public void Tick(float dt)
    {
        // Sadece wall slide sırasında wall jump yapılabilir
        if (fsm.Current != PlayerStateMachine.LocoState.WallSlide) return;
        
        // Wall jump input kontrolü
        if (input.JumpPressed)
        {
            int wallDir = 0;
            bool canJump = false;
            
            // Sol tarafta jumpable var mı?
            if (sensors.wallLeft && sensors.onJumpableLeft)
            {
                wallDir = -1;
                canJump = true;
            }
            // Sağ tarafta jumpable var mı?
            else if (sensors.wallRight && sensors.onJumpableRight)
            {
                wallDir = +1;
                canJump = true;
            }
            
            if (canJump)
            {
                // Wall jump hesaplama
                float angle = Mathf.Deg2Rad * (wallJumpCfg ? wallJumpCfg.wallJumpAngle : 55f);
                Vector2 outDir = new Vector2(-wallDir * Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                float impulse = wallJumpCfg ? wallJumpCfg.wallJumpImpulse : 10f;
                
                // Variable jump height kontrolü
                if (wallJumpCfg && wallJumpCfg.allowVariableJump && !input.JumpHeld)
                {
                    impulse *= wallJumpCfg.variableJumpMultiplier;
                }
                
                motor.RequestWallJump(outDir * impulse);
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpRise, "WallJump");
            }
        }
    }
}
