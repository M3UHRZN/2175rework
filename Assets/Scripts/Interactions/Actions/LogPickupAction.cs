using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Log Pickup Action")]
public class LogPickupAction : InteractActionBase
{
    [System.Serializable]
    public class LogEvent : UnityEvent<string> { }

    [Header("Log Pickup")]
    [SerializeField] string logId = "Lore/Entry01";
    [SerializeField] LogEvent onLogCollected = new LogEvent();
    [SerializeField] Transform collectedVisual;
    [SerializeField] Transform availableVisual;

    bool collected;

    protected override void Awake()
    {
        base.Awake();
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (collected)
            return;
        collected = true;
        onLogCollected.Invoke(logId);
        ApplyState();
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(collectedVisual, collected);
        InteractUtils.SetActiveSafe(availableVisual, !collected);
    }
}
