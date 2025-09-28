using UnityEngine;

[AddComponentMenu("Interactions/Actions/Time Lever Action")]
public class TimeLeverAction : InteractActionBase
{
    [SerializeField] float slowFactor = 0.5f;

    float originalScale = 1f;
    bool applied;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!applied)
            originalScale = Time.timeScale;
    }

    protected override void OnStart(InteractionController controller)
    {
        ApplySlow();
    }

    protected override void OnComplete(InteractionController controller)
    {
        Restore();
    }

    protected override void OnCancel(InteractionController controller)
    {
        Restore();
    }

    void ApplySlow()
    {
        if (applied)
            return;
        originalScale = Time.timeScale;
        Time.timeScale = Mathf.Max(0.01f, slowFactor);
        applied = true;
    }

    void Restore()
    {
        if (!applied)
            return;
        Time.timeScale = originalScale;
        applied = false;
    }
}
