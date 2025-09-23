using UnityEngine;

[DisallowMultipleComponent]
public class ClimbController : MonoBehaviour
{
    public ClimbConfig climbCfg;
    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;

    void Awake()
    {
        input  = GetComponent<InputAdapter>();
        sensors= GetComponent<Sensors2D>();
        motor  = GetComponent<LocomotionMotor2D>();
        fsm    = GetComponent<PlayerStateMachine>();
    }

    public void Tick(float dt)
    {
        if (sensors.onLadder)
        {
            if (Mathf.Abs(input.MoveY) > 0.1f)
            {
                float speed = climbCfg ? climbCfg.climbSpeed : 4f;
                // ladder volume override (ops)
                var hit = Physics2D.OverlapCircle(transform.position, 0.2f, sensors.ladderMask);
                var vol = hit ? hit.GetComponent<LadderVolume>() : null;
                if (vol && vol.overrideClimbSpeed > 0f) speed = vol.overrideClimbSpeed;

                motor.RequestClimb(input.MoveY * speed);
                
                // Sadece daha yüksek öncelikli state'e geçiş yapabilir
                if (fsm.CanTransitionTo(PlayerStateMachine.LocoState.Climb))
                {
                    fsm.RequestTransition(PlayerStateMachine.LocoState.Climb, "Climb");
                }
            }

            if (input.JumpPressed)
            {
                motor.RequestStopClimbRestoreGravity();
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpRise, "ClimbJump");
            }
        }
        else if (fsm.Current == PlayerStateMachine.LocoState.Climb)
        {
            motor.RequestStopClimbRestoreGravity();
            fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "ExitClimb");
        }
    }
}
