using UnityEngine;

[AddComponentMenu("Interactions/Actions/Slow Time Action")]
public class SlowTimeAction : InteractActionBase
{
    [SerializeField] float slowScale = 0.25f;
    float originalScale = 1f;
    bool active;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, false, false, false, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        if (active)
            return;
        originalScale = Time.timeScale;
        Time.timeScale = Mathf.Max(0.01f, slowScale);
        active = true;
    }

    protected override void OnCancel(InteractionController controller)
    {
        Restore();
    }

    protected override void OnFocusExit(InteractionController controller)
    {
        Restore();
    }

    void Restore()
    {
        if (!active)
            return;
        Time.timeScale = originalScale;
        active = false;
    }
}
