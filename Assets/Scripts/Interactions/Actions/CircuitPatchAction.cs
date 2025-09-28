using UnityEngine;

[AddComponentMenu("Interactions/Actions/Circuit Patch Action")]
public class CircuitPatchAction : InteractActionBase
{
    [SerializeField] GameObject glowRoot;
    [SerializeField] Color onColor = Color.cyan;
    [SerializeField] Color offColor = Color.gray;
    [SerializeField] SpriteRenderer indicator;

    bool patched;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ApplyState(patched);
    }

    protected override void OnComplete(InteractionController controller)
    {
        patched = !patched;
        ApplyState(patched);
    }

    void ApplyState(bool enabled)
    {
        InteractUtils.SetActiveSafe(glowRoot, enabled);
        if (indicator)
            indicator.color = enabled ? onColor : offColor;
    }
}
