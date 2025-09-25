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
    WallJumpController wallJump;
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
        wallJump = GetComponent<WallJumpController>();
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
        
        // 3. WallJump (WallSlide sırasında)
        wallJump?.Tick(dt);
        
        // 4. Climb (Ladder)
        climb?.Tick(dt);
        
        // 5. Jump
        jump?.Tick(dt);
        
        // 6. Interact
        interact?.Tick(dt);

        // Default state resolution - sadece düşük öncelikli state'lerde çalışır
        ResolveDefaultStates();
    }

    private void ResolveDefaultStates()
    {
        // Wall slide çıkış kontrolü
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

        // Wall climb çıkış kontrolü
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

        // Climb çıkış kontrolü
        if (fsm.Current == PlayerStateMachine.LocoState.Climb)
        {
            if (!sensors.onLadder)
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "Climb->Fall");
                return;
            }
        }

        // Default state resolution - sadece düşük öncelikli state'lerde
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
