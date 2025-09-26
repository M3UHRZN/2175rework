using UnityEngine;

[DisallowMultipleComponent]
public class AbilityController : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;
    PlayerStateHooks hooks;

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
        hooks  = GetComponent<PlayerStateHooks>();

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

        // State çıkış kontrollerini PlayerStateHooks'a delege et
        if (hooks != null)
        {
            hooks.CheckStateExits();
        }

        // Default state resolution - PlayerStateHooks'a delege et
        if (hooks != null)
        {
            hooks.ResolveDefaultStates();
        }
    }

}
