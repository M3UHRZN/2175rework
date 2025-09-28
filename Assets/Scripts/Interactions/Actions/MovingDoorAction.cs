using UnityEngine;

[AddComponentMenu("Interactions/Actions/Moving Door Action")]
public class MovingDoorAction : InteractActionBase
{
    [Header("Animator")]
    [SerializeField] Animator animator;
    [SerializeField] string openBool = "Open";
    [SerializeField] string callTrigger = "Call";

    bool isOpen;
    int openId;
    int triggerId;

    protected override void Awake()
    {
        base.Awake();
        openId = Animator.StringToHash(openBool);
        triggerId = Animator.StringToHash(callTrigger);
    }

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        openId = Animator.StringToHash(openBool);
        triggerId = Animator.StringToHash(callTrigger);
        ApplyState(isOpen);
    }

    protected override void OnComplete(InteractionController controller)
    {
        isOpen = true;
        ApplyState(true);
    }

    protected override void OnCancel(InteractionController controller)
    {
        isOpen = false;
        ApplyState(false);
    }

    void ApplyState(bool open)
    {
        InteractUtils.SetAnimatorBool(animator, openId, open);
        if (!open)
            animator?.ResetTrigger(triggerId);
        else if (triggerId != 0)
            InteractUtils.PlayAnimatorTrigger(animator, triggerId);
    }
}
