using UnityEngine;

[AddComponentMenu("Interactions/Actions/Valve Action")]
public class ValveAction : InteractActionBase
{
    [SerializeField] Transform valveHandle;
    [SerializeField] float maxAngle = 120f;

    float currentAngle;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, true, true, true);
    }

    protected override void OnProgress(float value)
    {
        ApplyAngle(value);
    }

    protected override void OnComplete(InteractionController controller)
    {
        ApplyAngle(1f);
    }

    protected override void OnCancel(InteractionController controller)
    {
        ApplyAngle(0f);
    }

    void ApplyAngle(float normalized)
    {
        if (!valveHandle)
            return;
        normalized = Mathf.Clamp01(normalized);
        currentAngle = Mathf.Lerp(0f, maxAngle, normalized);
        valveHandle.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
    }
}
