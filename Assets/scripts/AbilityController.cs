using UnityEngine;

[DisallowMultipleComponent]
public class AbilityController : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;

    JumpController jump;
    WallMovement wall;
    WallClimbController wallClimb;
    ClimbController climb;
    InteractionController interact;

    void Awake()
    {
        input  = GetComponent<InputAdapter>();
        sensors= GetComponent<Sensors2D>();
        motor  = GetComponent<LocomotionMotor2D>();
        fsm    = GetComponent<PlayerStateMachine>();

        jump  = GetComponent<JumpController>();
        wall  = GetComponent<WallMovement>();
        wallClimb = GetComponent<WallClimbController>();
        climb = GetComponent<ClimbController>();
        interact = GetComponent<InteractionController>();
    }

    public void Tick(float dt)
    {
        // Base locomotion intent (yatay)
        motor.RequestHorizontalIntent(input.MoveX);

        // Unified State Management - öncelik sırasına göre controller'ları çalıştır
        // Her controller sadece kendi state'ini request eder, FSM priority'ye göre karar verir
        
        
        // 1. WallClimb (WallSlide'dan önce)
        wallClimb?.Tick(dt);
        
        // 2. WallSlide (WallClimb'dan sonra)
        wall?.Tick(dt);
        
        // 3. Climb (Ladder)
        climb?.Tick(dt);
        
        // 4. Jump
        jump?.Tick(dt);
        
        // 5. Interact
        interact?.Tick(dt);

        // Default state resolution - sadece düşük öncelikli state'lerde çalışır
        ResolveDefaultStates();
    }

    private void ResolveDefaultStates()
    {
        // Sadece düşük öncelikli state'lerde default resolution yap
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
