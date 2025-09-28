using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Moving Door Action")]
public class MovingDoorAction : InteractActionBase
{
    [Header("Moving Door")]
    [SerializeField] Animator animator;
    [SerializeField] string openBoolName = "Open";
    [SerializeField] string callTriggerName = string.Empty;
    [Tooltip("Optional transform moved directly when no animator is supplied.")]
    [SerializeField] Transform movingRoot;
    [SerializeField] Vector3 closedLocalPosition;
    [SerializeField] Vector3 openLocalPosition;

    bool isOpen;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
        ApplyState(false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        isOpen = true;
        ApplyState(true);
    }

    protected override void OnCancel(InteractionController controller)
    {
        isOpen = false;
        ApplyState(true);
    }

    void ApplyState(bool animate)
    {
        if (animator)
        {
            if (!string.IsNullOrEmpty(openBoolName))
            {
                animator.SetBool(openBoolName, isOpen);
            }
            if (animate && !string.IsNullOrEmpty(callTriggerName))
            {
                animator.SetTrigger(callTriggerName);
            }
        }
        else if (movingRoot)
        {
            movingRoot.localPosition = isOpen ? openLocalPosition : closedLocalPosition;
        }
    }
}
