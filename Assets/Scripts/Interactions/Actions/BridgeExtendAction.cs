using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Bridge Extend Action")]
public class BridgeExtendAction : InteractActionBase
{
    [Header("Bridge")]
    [SerializeField] Transform bridgeRoot;
    [SerializeField] Collider2D bridgeCollider;
    [SerializeField] bool startExtended;

    bool extended;

    protected override void Awake()
    {
        base.Awake();
        extended = startExtended;
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        extended = !extended;
        ApplyState();
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(bridgeRoot, extended);
        InteractUtils.SetColliderPassable(bridgeCollider, extended);
    }
}
