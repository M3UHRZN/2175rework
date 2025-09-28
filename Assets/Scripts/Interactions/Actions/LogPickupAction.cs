using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Log Pickup Action")]
public class LogPickupAction : InteractActionBase
{
    [SerializeField] string logId;
    [SerializeField] UnityEvent onCollected;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (!string.IsNullOrEmpty(logId))
            InteractionStateRegistry.SetBool(logId, true);
        onCollected?.Invoke();
    }
}
