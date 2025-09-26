using UnityEngine;
using System;

[DisallowMultipleComponent]
public class PlayerStateMachine : MonoBehaviour
{
    public enum LocoState { Idle, Run, JumpRise, JumpFall, WallSlide, WallClimb, Climb }
    public enum PhaseState { None, WallJump }

    public LocoState Current { get; private set; } = LocoState.Idle;
    public PhaseState Phase { get; private set; } = PhaseState.None;

    public event Action<LocoState, LocoState> OnLocoChanged;
    public event Action<PhaseState> OnPhaseTriggered;

    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateHooks hooks;

    void Awake()
    {
        sensors = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        hooks = GetComponent<PlayerStateHooks>();
    }

    // State priority system - yüksek sayı = yüksek öncelik
    private static readonly int[] StatePriorities = new int[]
    {
        0, // Idle
        0, // Run  
        1, // JumpRise
        1, // JumpFall
        2, // WallSlide
        3, // WallClimb
        2  // Climb
    };

    public void ForceSet(LocoState s)
    {
        if (s == Current) return;
        var old = Current;
        Current = s;
        OnLocoChanged?.Invoke(old, s);
    }

    public bool RequestTransition(LocoState target, string reason = "")
    {
        if (target == Current) return false;
        
        // Context-aware transition rules
        if (CanTransitionFromTo(Current, target))
        {
            // State çıkış mantığı - PlayerStateHooks'a delege et
            if (hooks != null)
            {
                hooks.HandleStateExit(Current);
            }
            
            ForceSet(target);
            return true;
        }
        
        return false;
    }


    private bool CanTransitionFromTo(LocoState from, LocoState to)
    {
        // Her state'ten hangi state'lere geçilebileceğini tanımla
        switch (from)
        {
            case LocoState.Idle:
            case LocoState.Run:
                return true; // Ground state'lerden her yere geçilebilir
                
            case LocoState.JumpRise:
                return to == LocoState.JumpFall || to == LocoState.Idle || to == LocoState.Run || 
                       to == LocoState.WallSlide || to == LocoState.WallClimb || 
                       to == LocoState.Climb;
                       
            case LocoState.JumpFall:
                return to == LocoState.Idle || to == LocoState.Run || 
                       to == LocoState.WallSlide || to == LocoState.WallClimb || 
                       to == LocoState.Climb;
                       
            case LocoState.WallSlide:
                return to == LocoState.JumpRise || to == LocoState.JumpFall || 
                       to == LocoState.WallClimb || to == LocoState.Idle || to == LocoState.Run;
                       
            case LocoState.WallClimb:
                return to == LocoState.JumpRise || to == LocoState.JumpFall || 
                       to == LocoState.WallSlide;
                       
                       
            case LocoState.Climb:
                return to == LocoState.JumpRise || to == LocoState.JumpFall;
                
            default:
                return false;
        }
    }

    public bool CanTransitionTo(LocoState target)
    {
        if (target == Current) return false;
        return CanTransitionFromTo(Current, target);
    }

    public void TriggerPhase(PhaseState p)
    {
        Phase = p;
        OnPhaseTriggered?.Invoke(p);
        Phase = PhaseState.None;
    }
}
