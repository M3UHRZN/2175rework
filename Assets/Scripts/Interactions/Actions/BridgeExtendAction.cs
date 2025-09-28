using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Bridge Extend Action")]
public class BridgeExtendAction : InteractActionBase
{
    [SerializeField] GameObject bridgeRoot;
    [SerializeField] UnityEvent onExtended;
    [SerializeField] UnityEvent onRetracted;

    bool extended;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InteractUtils.SetActiveSafe(bridgeRoot, extended);
    }

    protected override void OnComplete(InteractionController controller)
    {
        extended = !extended;
        InteractUtils.SetActiveSafe(bridgeRoot, extended);
        if (extended)
            onExtended?.Invoke();
        else
            onRetracted?.Invoke();
    }
}
