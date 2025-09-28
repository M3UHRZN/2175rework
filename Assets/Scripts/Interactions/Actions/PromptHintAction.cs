using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Prompt Hint Action")]
public class PromptHintAction : InteractActionBase
{
    [Header("Prompt Hint")]
    [SerializeField] CanvasGroup worldSpaceHint;
    [SerializeField] CanvasGroup screenSpaceHint;

    UnityAction<InteractionController> focusEnterHandler;
    UnityAction<InteractionController> focusExitHandler;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(false, false, false, false);
        focusEnterHandler = HandleFocusEnter;
        focusExitHandler = HandleFocusExit;
        ApplyVisibility(false);
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
        ApplyVisibility(false);
        base.OnDisable();
    }

    void HandleFocusEnter(InteractionController controller)
    {
        ApplyVisibility(true);
    }

    void HandleFocusExit(InteractionController controller)
    {
        ApplyVisibility(false);
    }

    void ApplyVisibility(bool visible)
    {
        InteractUtils.SetCanvasGroupVisible(worldSpaceHint, visible);
        InteractUtils.SetCanvasGroupVisible(screenSpaceHint, visible);
    }
}
