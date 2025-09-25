using UnityEngine;

[DisallowMultipleComponent]
public class WallMovement : MonoBehaviour
{
    public WallConfig wallCfg;
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
        // Wall slide giriş kontrolü
        if (sensors.isGrounded) return;
        
        // WallClimb aktifse WallSlide'a geçmeye çalışma (çakışma önleme)
        if (fsm.Current == PlayerStateMachine.LocoState.WallClimb) return;

        int dir = 0;
        if (sensors.wallLeft  && input.MoveX < -0.1f) dir = -1;
        if (sensors.wallRight && input.MoveX > +0.1f) dir = +1;

        if (dir != 0)
        {
            motor.RequestWallSlide(dir);
            fsm.RequestTransition(PlayerStateMachine.LocoState.WallSlide, "WallSlide");
        }
    }
}
