using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Gizmo Pulse Action")]
public class GizmoPulseAction : InteractActionBase
{
    [Header("Pulse")]
    [SerializeField] Transform pulseGraphic;
    [SerializeField] float focusedScale = 1.2f;
    [SerializeField] float unfocusedScale = 1f;

    UnityAction<InteractionController> focusEnterHandler;
    UnityAction<InteractionController> focusExitHandler;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(false, false, false, false);
        focusEnterHandler = _ => ApplyScale(focusedScale);
        focusExitHandler = _ => ApplyScale(unfocusedScale);
        ApplyScale(unfocusedScale);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var interactable = Target;
        if (interactable != null)
        {
            interactable.OnFocusEnter.AddListener(focusEnterHandler);
            interactable.OnFocusExit.AddListener(focusExitHandler);
        }
    }

    protected override void OnDisable()
    {
        var interactable = Target;
        if (interactable != null)
        {
            interactable.OnFocusEnter.RemoveListener(focusEnterHandler);
            interactable.OnFocusExit.RemoveListener(focusExitHandler);
        }
        ApplyScale(unfocusedScale);
        base.OnDisable();
    }

    void ApplyScale(float scale)
    {
        if (!pulseGraphic)
            return;
        pulseGraphic.localScale = Vector3.one * scale;
    }
}
