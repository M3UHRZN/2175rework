using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Valve Action")]
public class ValveAction : InteractActionBase
{
    [Header("Valve")]
    [SerializeField] Transform valveRoot;
    [SerializeField] float maxAngle = 180f;
    [SerializeField] bool resetOnCancel = true;

    float currentValue;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, true, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        currentValue = 0f;
        ApplyRotation();
    }

    protected override void OnProgress(float value)
    {
        currentValue = Mathf.Clamp01(value);
        ApplyRotation();
    }

    protected override void OnComplete(InteractionController controller)
    {
        currentValue = 1f;
        ApplyRotation();
    }

    protected override void OnCancel(InteractionController controller)
    {
        if (resetOnCancel)
        {
            currentValue = 0f;
            ApplyRotation();
        }
    }

    void ApplyRotation()
    {
        if (!valveRoot)
            return;
        valveRoot.localRotation = Quaternion.Euler(0f, 0f, -maxAngle * currentValue);
    }
}
