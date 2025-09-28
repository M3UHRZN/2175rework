using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Slow Time Action")]
public class SlowTimeAction : InteractActionBase
{
    [Header("Slow Time")]
    [SerializeField] float timeScale = 0.5f;
    [SerializeField] bool restoreOnComplete = true;

    float defaultTimeScale = 1f;
    bool applied;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        Apply();
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (restoreOnComplete)
            Restore();
    }

    protected override void OnCancel(InteractionController controller)
    {
        Restore();
    }

    void Apply()
    {
        if (applied)
            return;
        defaultTimeScale = Time.timeScale;
        Time.timeScale = Mathf.Max(0.01f, timeScale);
        applied = true;
    }

    void Restore()
    {
        if (!applied)
            return;
        Time.timeScale = defaultTimeScale;
        applied = false;
    }

    protected override void OnDisable()
    {
        Restore();
        base.OnDisable();
    }
}
