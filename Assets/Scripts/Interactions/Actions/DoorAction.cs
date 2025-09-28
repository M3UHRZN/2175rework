using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Door Action")]
public class DoorAction : InteractActionBase
{
    [Header("Door Setup")]
    [Tooltip("Optional closed state root.")]
    [SerializeField] Transform closedVisual;
    [Tooltip("Optional open state root.")]
    [SerializeField] Transform openVisual;
    [SerializeField] Collider2D blockingCollider;
    [Tooltip("Optional animator controlling an 'Open' bool parameter.")]
    [SerializeField] Animator animator;
    [SerializeField] string openBoolName = "Open";
    [SerializeField] bool startOpen;

    bool isOpen;

    protected override void Awake()
    {
        base.Awake();
        isOpen = startOpen;
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        isOpen = !isOpen;
        ApplyState();
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(openVisual, isOpen);
        InteractUtils.SetActiveSafe(closedVisual, !isOpen);
        InteractUtils.SetColliderPassable(blockingCollider, isOpen);
        if (animator && !string.IsNullOrEmpty(openBoolName))
        {
            animator.SetBool(openBoolName, isOpen);
        }
    }
}
