using UnityEngine;
using UnityEngine.Profiling;

[AddComponentMenu("Interactions/Actions/Log Event Action")]
public class LogEventAction : InteractActionBase
{
    [SerializeField] string contextLabel;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, true, true, true, true, true);
    }

    protected override void OnFocusEnter(InteractionController controller)
    {
        Log("FocusEnter", controller);
    }

    protected override void OnFocusExit(InteractionController controller)
    {
        Log("FocusExit", controller);
    }

    protected override void OnStart(InteractionController controller)
    {
        Log("Start", controller);
    }

    protected override void OnProgress(float value)
    {
        Log($"Progress:{value:0.00}", ActiveController);
    }

    protected override void OnComplete(InteractionController controller)
    {
        Log("Complete", controller);
    }

    protected override void OnCancel(InteractionController controller)
    {
        Log("Cancel", controller);
    }

    void Log(string eventName, InteractionController controller)
    {
        string label = string.IsNullOrEmpty(contextLabel) ? name : contextLabel;
        string actor = controller ? controller.actor.ToString() : "None";
        string message = $"[Interaction] {label} -> {eventName} (Actor: {actor})";
        Profiler.BeginSample(message);
        Debug.Log(message, this);
        Profiler.EndSample();
    }
}
