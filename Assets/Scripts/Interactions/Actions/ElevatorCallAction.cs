using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Elevator Call Action")]
public class ElevatorCallAction : InteractActionBase
{
    [SerializeField] Animator animator;
    [SerializeField] string callTrigger = "Call";
    [SerializeField] UnityEvent onCall;

    int triggerId;

    protected override void Awake()
    {
        base.Awake();
        triggerId = Animator.StringToHash(callTrigger);
    }

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        triggerId = Animator.StringToHash(callTrigger);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (triggerId != 0)
            InteractUtils.PlayAnimatorTrigger(animator, triggerId);
        onCall?.Invoke();
    }
}
