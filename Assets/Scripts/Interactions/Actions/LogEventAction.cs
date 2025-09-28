using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Log Event Action")]
public class LogEventAction : InteractActionBase
{
    [Header("Logging Settings")]
    [SerializeField] bool logStart = true;
    [SerializeField] bool logProgress = false;
    [SerializeField] bool logComplete = true;
    [SerializeField] bool logCancel = true;
    [SerializeField] bool includeTimestamp = true;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, true, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        if (logStart)
        {
            string message = $"Interaction start: {name}";
            if (includeTimestamp)
                message += $" [Time: {Time.time:F2}]";
            Debug.Log(message);
        }
    }

    protected override void OnProgress(float value)
    {
        if (logProgress)
        {
            string message = $"Interaction progress {name}: {value:0.00}";
            if (includeTimestamp)
                message += $" [Time: {Time.time:F2}]";
            Debug.Log(message);
        }
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (logComplete)
        {
            string message = $"Interaction complete: {name}";
            if (includeTimestamp)
                message += $" [Time: {Time.time:F2}]";
            Debug.Log(message);
        }
    }

    protected override void OnCancel(InteractionController controller)
    {
        if (logCancel)
        {
            string message = $"Interaction cancel: {name}";
            if (includeTimestamp)
                message += $" [Time: {Time.time:F2}]";
            Debug.Log(message);
        }
    }
}
