using UnityEngine;

[AddComponentMenu("Interactions/Actions/Door Action")]
public class DoorAction : InteractActionBase
{
    [Header("Door Elements")]
    [SerializeField] Transform closedVisual;
    [SerializeField] Transform openVisual;
    [SerializeField] SpriteRenderer closedSprite;
    [SerializeField] SpriteRenderer openSprite;
    [SerializeField] Collider2D doorCollider;
    [SerializeField] Animator animator;
    [SerializeField] string animatorOpenBool = "Open";

    bool isOpen;
    int animatorOpenId;

    protected override void Awake()
    {
        base.Awake();
        animatorOpenId = Animator.StringToHash(animatorOpenBool);
    }

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        animatorOpenId = Animator.StringToHash(animatorOpenBool);
        ApplyState(isOpen);
    }

    protected override void OnComplete(InteractionController controller)
    {
        Toggle();
    }

    void ApplyState(bool open)
    {
        InteractUtils.SetActiveSafe(openVisual, open);
        InteractUtils.SetActiveSafe(closedVisual, !open);
        InteractUtils.SetSpriteEnabled(openSprite, open);
        InteractUtils.SetSpriteEnabled(closedSprite, !open);
        if (doorCollider)
            doorCollider.enabled = !open;
        InteractUtils.SetAnimatorBool(animator, animatorOpenId, open);
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        ApplyState(isOpen);
    }

    public void Toggle(InteractionController controller)
    {
        Toggle();
    }
}
