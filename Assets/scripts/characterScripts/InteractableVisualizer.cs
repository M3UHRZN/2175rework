using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Interactable))]
[RequireComponent(typeof(SpriteRenderer))]
public class InteractableVisualizer : MonoBehaviour
{
    [Header("Colors")]
    public Color idleColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    public Color focusColor = Color.white;
    public Color activeColor = new Color(0.25f, 0.8f, 0.4f, 1f);
    public Color lockedColor = new Color(0.7f, 0.3f, 0.3f, 1f);

    SpriteRenderer spriteRenderer;
    Interactable interactable;
    bool hasFocus;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        interactable = GetComponent<Interactable>();

        if (interactable)
        {
            interactable.OnFocusEnter.AddListener(HandleFocusEnter);
            interactable.OnFocusExit.AddListener(HandleFocusExit);
            interactable.OnInteractStart.AddListener(HandleInteractStart);
            interactable.OnInteractComplete.AddListener(HandleInteractComplete);
            interactable.OnInteractCancel.AddListener(HandleInteractCancel);
        }
    }

    void OnEnable()
    {
        hasFocus = false;
        UpdateVisual();
    }

    void OnDestroy()
    {
        if (!interactable)
            return;

        interactable.OnFocusEnter.RemoveListener(HandleFocusEnter);
        interactable.OnFocusExit.RemoveListener(HandleFocusExit);
        interactable.OnInteractStart.RemoveListener(HandleInteractStart);
        interactable.OnInteractComplete.RemoveListener(HandleInteractComplete);
        interactable.OnInteractCancel.RemoveListener(HandleInteractCancel);
    }

    void HandleFocusEnter(InteractionController controller)
    {
        hasFocus = true;
        SetColor(focusColor);
    }

    void HandleFocusExit(InteractionController controller)
    {
        hasFocus = false;
        UpdateVisual();
    }

    void HandleInteractStart(InteractionController controller)
    {
        hasFocus = true;
        SetColor(focusColor);
    }

    void HandleInteractComplete(InteractionController controller)
    {
        hasFocus = false;
        UpdateVisual();
    }

    void HandleInteractCancel(InteractionController controller)
    {
        hasFocus = false;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (!spriteRenderer)
            return;

        if (interactable && interactable.IsLocked)
        {
            SetColor(lockedColor);
        }
        else if (interactable && interactable.ToggleState)
        {
            SetColor(activeColor);
        }
        else if (hasFocus)
        {
            SetColor(focusColor);
        }
        else
        {
            SetColor(idleColor);
        }
    }

    void SetColor(Color color)
    {
        if (spriteRenderer)
            spriteRenderer.color = color;
    }
}
