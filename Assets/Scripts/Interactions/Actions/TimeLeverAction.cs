using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Time Lever Action")]
public class TimeLeverAction : InteractActionBase
{
    [Header("Time Lever")]
    [SerializeField] float slowFactor = 0.25f;
    [SerializeField] bool affectFixedDeltaTime = true;

    float defaultTimeScale = 1f;
    float defaultFixedDeltaTime;
    bool applied;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    protected override void OnStart(InteractionController controller)
    {
        ApplyTimeScale();
    }

    protected override void OnComplete(InteractionController controller)
    {
        RestoreTimeScale();
    }

    protected override void OnCancel(InteractionController controller)
    {
        RestoreTimeScale();
    }

    void ApplyTimeScale()
    {
        if (applied)
            return;

        defaultTimeScale = Time.timeScale;
        Time.timeScale = Mathf.Max(0.01f, slowFactor);
        if (affectFixedDeltaTime)
            Time.fixedDeltaTime = defaultFixedDeltaTime * Time.timeScale;
        applied = true;
    }

    void RestoreTimeScale()
    {
        if (!applied)
            return;

        Time.timeScale = defaultTimeScale;
        if (affectFixedDeltaTime)
            Time.fixedDeltaTime = defaultFixedDeltaTime;
        applied = false;
    }

    protected override void OnDisable()
    {
        RestoreTimeScale();
        base.OnDisable();
    }
}
