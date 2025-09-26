using UnityEngine;

[DisallowMultipleComponent]
public class InteractionIndicator : MonoBehaviour
{
    [Tooltip("Sprite renderer that will be tinted when the state changes.")]
    public SpriteRenderer targetRenderer;
    [Tooltip("Color used when the interaction is inactive or locked.")]
    public Color inactiveColor = new Color(0.8f, 0.2f, 0.2f);
    [Tooltip("Color used when the interaction has been completed or powered.")]
    public Color activeColor = new Color(0.2f, 0.85f, 0.4f);

    void Awake()
    {
        if (!targetRenderer)
            targetRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetState(bool value)
    {
        if (!targetRenderer)
            return;

        targetRenderer.color = value ? activeColor : inactiveColor;
        if (!targetRenderer.enabled)
            targetRenderer.enabled = true;
    }
}
