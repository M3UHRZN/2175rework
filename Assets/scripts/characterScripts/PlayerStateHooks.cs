using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateHooks : MonoBehaviour
{
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
        if (!fsm || !sensors || !input || !motor)
            return;

        if (fsm.Current == PlayerStateMachine.LocoState.WallSlide)
        {
            if (sensors.isGrounded)
            {
                fsm.RequestTransition(Mathf.Abs(input.MoveX) > 0.05f
                    ? PlayerStateMachine.LocoState.Run
                    : PlayerStateMachine.LocoState.Idle, "WallSlide->Grounded");
                return;
            }

            if (!sensors.wallLeft && !sensors.wallRight)
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "WallSlide->NoWall");
                return;
            }
        }

        if (fsm.Current == PlayerStateMachine.LocoState.WallClimb)
        {
            if (!(sensors.onClimbableAny && (sensors.wallLeft || sensors.wallRight)))
            {
                if (!sensors.isGrounded && (sensors.wallLeft || sensors.wallRight))
                    fsm.RequestTransition(PlayerStateMachine.LocoState.WallSlide, "WallClimb->Slide");
                else
                    fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "WallClimb->Fall");
                return;
            }
        }

        if (fsm.Current == PlayerStateMachine.LocoState.Climb)
        {
            if (!sensors.onLadder)
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "Climb->Fall");
                return;
            }
        }

        if (fsm.Current == PlayerStateMachine.LocoState.Idle ||
            fsm.Current == PlayerStateMachine.LocoState.Run ||
            fsm.Current == PlayerStateMachine.LocoState.JumpRise ||
            fsm.Current == PlayerStateMachine.LocoState.JumpFall)
        {
            if (sensors.isGrounded)
            {
                fsm.RequestTransition(Mathf.Abs(input.MoveX) > 0.05f
                    ? PlayerStateMachine.LocoState.Run
                    : PlayerStateMachine.LocoState.Idle, "Grounded");
            }
            else
            {
                fsm.RequestTransition(motor.velocityY > 0f
                    ? PlayerStateMachine.LocoState.JumpRise
                    : PlayerStateMachine.LocoState.JumpFall, "Airborne");
            }
        }
    }
}
