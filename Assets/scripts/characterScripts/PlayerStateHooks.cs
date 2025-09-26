using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerStateHooks : MonoBehaviour
{
    [Header("Optional References")]
    public AnimationFacade animationFacade;
    public Sensors2D sensors;
    public LocomotionMotor2D locomotionMotor;

    PlayerStateMachine stateMachine;

    void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        if (!animationFacade) animationFacade = GetComponent<AnimationFacade>();
        if (!sensors) sensors = GetComponent<Sensors2D>();
        if (!locomotionMotor) locomotionMotor = GetComponent<LocomotionMotor2D>();
    }

    void OnEnable()
    {
        if (stateMachine == null) return;
        stateMachine.OnLocoChanged += HandleLocoChanged;
        stateMachine.OnPhaseTriggered += HandlePhaseTriggered;
    }

    void OnDisable()
    {
        if (stateMachine == null) return;
        stateMachine.OnLocoChanged -= HandleLocoChanged;
        stateMachine.OnPhaseTriggered -= HandlePhaseTriggered;
    }

    void HandleLocoChanged(PlayerStateMachine.LocoState previous, PlayerStateMachine.LocoState next)
    {
        if (animationFacade == null || animationFacade.animator == null) return;

        if (next == PlayerStateMachine.LocoState.JumpRise)
        {
            animationFacade.Trigger("Jump");
        }
        else if ((next == PlayerStateMachine.LocoState.Idle || next == PlayerStateMachine.LocoState.Run) && sensors != null && sensors.justLanded)
        {
            animationFacade.Trigger("Land");
        }
    }

    void HandlePhaseTriggered(PlayerStateMachine.PhaseState phase)
    {
        if (animationFacade == null || animationFacade.animator == null) return;

        if (phase == PlayerStateMachine.PhaseState.WallJump)
        {
            animationFacade.Trigger("WallJump");
        }
    }
}
