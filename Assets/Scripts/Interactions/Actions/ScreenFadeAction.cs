using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Screen Fade Action")]
public class ScreenFadeAction : InteractActionBase
{
    [Header("Screen Fade")]
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float startAlpha = 0f;
    [SerializeField] float endAlpha = 1f;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
        ApplyAlpha(startAlpha);
    }

    protected override void OnStart(InteractionController controller)
    {
        ApplyAlpha(endAlpha);
    }

    protected override void OnComplete(InteractionController controller)
    {
        ApplyAlpha(endAlpha);
    }

    protected override void OnCancel(InteractionController controller)
    {
        ApplyAlpha(startAlpha);
    }

    void ApplyAlpha(float alpha)
    {
        if (!canvasGroup)
            return;
        canvasGroup.alpha = Mathf.Clamp01(alpha);
    }
}
