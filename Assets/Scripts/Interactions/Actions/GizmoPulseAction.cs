using UnityEngine;

[AddComponentMenu("Interactions/Actions/Gizmo Pulse Action")]
public class GizmoPulseAction : InteractActionBase
{
    [SerializeField] Transform pulseTarget;
    [SerializeField] float pulseScale = 1.2f;
    [SerializeField] float returnSpeed = 6f;

    Vector3 originalScale;
    bool pulsing;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, true, false, false, false, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!pulseTarget)
            pulseTarget = transform;
        originalScale = pulseTarget ? pulseTarget.localScale : Vector3.one;
    }

    protected override void OnFocusEnter(InteractionController controller)
    {
        if (!pulseTarget)
            return;
        pulseTarget.localScale = originalScale * pulseScale;
        pulsing = true;
        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), 0.5f);
    }

    protected override void OnFocusExit(InteractionController controller)
    {
        ResetScale();
    }

    void ResetScale()
    {
        if (!pulseTarget)
            return;
        pulseTarget.localScale = Vector3.Lerp(pulseTarget.localScale, originalScale, returnSpeed * Time.deltaTime);
        pulseTarget.localScale = originalScale;
        pulsing = false;
    }
}
