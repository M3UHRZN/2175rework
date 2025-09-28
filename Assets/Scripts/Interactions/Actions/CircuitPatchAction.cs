using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Circuit Patch Action")]
public class CircuitPatchAction : InteractActionBase
{
    [Header("Circuit")]
    [SerializeField] Renderer[] glowRenderers;
    [SerializeField] Color onColor = Color.cyan;
    [SerializeField] Color offColor = Color.gray;

    bool active;

    protected override void Awake()
    {
        base.Awake();
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        active = !active;
        ApplyState();
    }

    static readonly int colorProperty = Shader.PropertyToID("_Color");
    static readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

    void ApplyState()
    {
        if (glowRenderers == null)
            return;

        Color color = active ? onColor : offColor;
        for (int i = 0; i < glowRenderers.Length; i++)
        {
            var renderer = glowRenderers[i];
            if (!renderer)
                continue;
            propertyBlock.Clear();
            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(colorProperty, color);
            renderer.SetPropertyBlock(propertyBlock);
            renderer.enabled = true;
        }
    }
}
