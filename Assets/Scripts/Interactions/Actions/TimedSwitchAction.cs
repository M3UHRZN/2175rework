using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Timed Switch Action")]
public class TimedSwitchAction : InteractActionBase
{
    [SerializeField] float durationMs = 2000f;
    [SerializeField] UnityEvent onActivate;
    [SerializeField] UnityEvent onDeactivate;

    bool active;
    float durationSeconds;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
        durationSeconds = durationMs / 1000f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        durationSeconds = durationMs / 1000f;
    }

    protected override void OnComplete(InteractionController controller)
    {
        Activate();
    }

    void Activate()
    {
        active = true;
        onActivate?.Invoke();
        CancelInvoke(nameof(Deactivate));
        durationSeconds = Mathf.Max(0.01f, durationMs / 1000f);
        Invoke(nameof(Deactivate), durationSeconds);
    }

    void Deactivate()
    {
        active = false;
        onDeactivate?.Invoke();
    }
}
