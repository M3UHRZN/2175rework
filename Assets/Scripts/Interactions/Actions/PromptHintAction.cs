using UnityEngine;

[AddComponentMenu("Interactions/Actions/Prompt Hint Action")]
public class PromptHintAction : InteractActionBase
{
    [SerializeField] GameObject worldSpaceHint;
    [SerializeField] CanvasGroup screenSpaceHint;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, true, false, false, false, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetVisible(false);
    }

    protected override void OnFocusEnter(InteractionController controller)
    {
        SetVisible(true);
    }

    protected override void OnFocusExit(InteractionController controller)
    {
        SetVisible(false);
    }

    void SetVisible(bool visible)
    {
        InteractUtils.SetActiveSafe(worldSpaceHint, visible);
        InteractUtils.SetCanvasGroupAlpha(screenSpaceHint, visible ? 1f : 0f);
    }
}
