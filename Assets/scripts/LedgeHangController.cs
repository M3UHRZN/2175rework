using UnityEngine;

[DisallowMultipleComponent]
public class LedgeHangController : MonoBehaviour
{
    public LedgeConfig ledgeCfg;
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
        if (sensors.isGrounded) return;

        // Grab
        if (sensors.isLedge && fsm.Current != PlayerStateMachine.LocoState.LedgeHang)
        {
            Vector2 snap = sensors.activeLedge ? sensors.activeLedge.WorldHangPoint
                                               : (Vector2)transform.position; // fallback
            int face = sensors.activeLedge ? Mathf.Clamp(sensors.activeLedge.preferFacing, -1, 1) : 0;
            motor.RequestLedgeHang(snap, face);
            
            // Sadece daha yüksek öncelikli state'e geçiş yapabilir
            if (fsm.CanTransitionTo(PlayerStateMachine.LocoState.LedgeHang))
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.LedgeHang, "LedgeHang");
            }
            return;
        }

        if (fsm.Current == PlayerStateMachine.LocoState.LedgeHang)
        {
            // Up or toward edge → climb
            int dir = input.MoveX > 0.2f ? 1 : (input.MoveX < -0.2f ? -1 : 0);
            bool goUp = input.MoveY > 0.2f || dir != 0;

            if (goUp)
            {
                Vector2 shift = sensors.activeLedge ? sensors.activeLedge.climbUpShift
                                                    : (ledgeCfg ? ledgeCfg.defaultClimbShift : new Vector2(0.5f, 0.9f));
                if (dir == 0) dir = 1; // default right
                motor.RequestLedgeClimb(dir, shift);
                fsm.RequestTransition(PlayerStateMachine.LocoState.Idle, "LedgeClimb");
            }
            else if (input.DownHeld)
            {
                // drop
                motor.RequestStopClimbRestoreGravity();
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "LedgeDrop");
            }
        }
    }
}
