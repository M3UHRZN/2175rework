using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteStateIndicator : MonoBehaviour
{
    [Header("Colors")]
    public Color offColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    public Color onColor = new Color(0.2f, 0.85f, 0.45f, 1f);

    SpriteRenderer spriteRenderer;
    bool currentState;

    void Awake()
    {
        EnsureRenderer();
        ApplyColor(currentState);
    }

    void OnEnable()
    {
        ApplyColor(currentState);
    }

    void Reset()
    {
        EnsureRenderer();
        ApplyColor(currentState);
    }

    void EnsureRenderer()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void ApplyColor(bool state)
    {
        EnsureRenderer();
        if (spriteRenderer)
            spriteRenderer.color = state ? onColor : offColor;
    }

    public void SetState(bool state)
    {
        currentState = state;
        ApplyColor(state);
    }
}
