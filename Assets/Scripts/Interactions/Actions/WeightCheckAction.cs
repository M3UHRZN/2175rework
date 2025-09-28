using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Weight Check Action")]
public class WeightCheckAction : InteractActionBase
{
    [SerializeField] Collider2D detectionArea;
    [SerializeField] float massThreshold = 10f;
    [SerializeField] UnityEvent onThresholdPassed;
    [SerializeField] UnityEvent onThresholdFailed;

    readonly Collider2D[] overlapBuffer = new Collider2D[16];

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (!detectionArea)
        {
            onThresholdFailed?.Invoke();
            return;
        }

        var filter = new ContactFilter2D { useTriggers = true, useLayerMask = false, useDepth = false };
        int count = detectionArea.OverlapCollider(filter, overlapBuffer);
        float totalMass = 0f;
        for (int i = 0; i < count; i++)
        {
            var col = overlapBuffer[i];
            if (!col)
                continue;
            var rb = col.attachedRigidbody;
            if (rb)
                totalMass += rb.mass;
        }

        if (totalMass >= massThreshold)
            onThresholdPassed?.Invoke();
        else
            onThresholdFailed?.Invoke();
    }
}
