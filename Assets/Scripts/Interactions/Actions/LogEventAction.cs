using UnityEngine;
using UnityEngine.Profiling;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Log Event Action")]
public class LogEventAction : InteractActionBase
{
    static readonly ProfilerMarker startMarker = new ProfilerMarker("Interaction.Start");
    static readonly ProfilerMarker progressMarker = new ProfilerMarker("Interaction.Progress");
    static readonly ProfilerMarker completeMarker = new ProfilerMarker("Interaction.Complete");
    static readonly ProfilerMarker cancelMarker = new ProfilerMarker("Interaction.Cancel");

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, true, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        using (startMarker.Auto())
        {
            Debug.Log($"Interaction start: {name}");
        }
    }

    protected override void OnProgress(float value)
    {
        using (progressMarker.Auto())
        {
            Debug.Log($"Interaction progress {name}: {value:0.00}");
        }
    }

    protected override void OnComplete(InteractionController controller)
    {
        using (completeMarker.Auto())
        {
            Debug.Log($"Interaction complete: {name}");
        }
    }

    protected override void OnCancel(InteractionController controller)
    {
        using (cancelMarker.Auto())
        {
            Debug.Log($"Interaction cancel: {name}");
        }
    }
}
