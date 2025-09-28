using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Weight Check Action")]
public class WeightCheckAction : InteractActionBase
{
    [Header("Weight Check")]
    [SerializeField] Collider2D checkArea;
    [SerializeField] float requiredMass = 10f;
    [SerializeField] LayerMask massMask = ~0;
    [SerializeField] Transform passedVisual;
    [SerializeField] Transform failedVisual;

    static readonly Collider2D[] overlapBuffer = new Collider2D[16];
    readonly ContactFilter2D contactFilter = new ContactFilter2D();

    protected override void Awake()
    {
        base.Awake();
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = massMask;
        contactFilter.useTriggers = true;
        ApplyState(false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        bool success = CheckMass();
        ApplyState(success);
    }

    bool CheckMass()
    {
        if (!checkArea)
            return false;

        int count = checkArea.OverlapCollider(contactFilter, overlapBuffer);
        float totalMass = 0f;
        for (int i = 0; i < count; i++)
        {
            var collider = overlapBuffer[i];
            if (!collider)
                continue;
            var body = collider.attachedRigidbody;
            if (!body)
                continue;
            totalMass += body.mass;
        }
        return totalMass >= requiredMass;
    }

    void ApplyState(bool success)
    {
        InteractUtils.SetActiveSafe(passedVisual, success);
        InteractUtils.SetActiveSafe(failedVisual, !success);
    }
}
