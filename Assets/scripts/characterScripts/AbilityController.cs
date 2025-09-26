using UnityEngine;

[DisallowMultipleComponent]
public class AbilityController : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    AbilityLoadout loadout;
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
        loadout = GetComponent<AbilityLoadout>();
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
        AbilitySnapshot abilities = loadout ? loadout.ActiveSnapshot : AbilitySnapshot.AllEnabled;

        bool movementLocked = interact && interact.MovementLocked;

        if (abilities.canMove && !movementLocked)
            motor.RequestHorizontalIntent(input.MoveX);
        else
            motor.RequestHorizontalIntent(0f);

        // Unified State Management - öncelik sırasına göre controller'ları çalıştır
        // Her controller sadece kendi state'ini request eder, FSM priority'ye göre karar verir
        
        
        // 1. WallClimb (WallSlide'dan önce)
        if (!loadout || (abilities.canWallClimb && !movementLocked))
            wallClimb?.Tick(dt);
        
        // 2. WallSlide (WallClimb'dan sonra)
        if (!loadout || (abilities.canWallSlide && !movementLocked))
            wall?.Tick(dt);
        
        // 3. WallJump (WallSlide sırasında)
        if (!loadout || (abilities.canWallJump && !movementLocked))
            wallJump?.Tick(dt);
        
        // 4. Climb (Ladder)
        if (!loadout || (abilities.canClimb && !movementLocked))
            climb?.Tick(dt);
        
        // 5. Jump
        if (!loadout || (abilities.canJump && !movementLocked))
            jump?.Tick(dt);
        
        // 6. Interact
        if (!loadout || abilities.canInteract)
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
