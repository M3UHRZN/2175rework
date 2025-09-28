using UnityEngine;

public enum InteractionAbilityRequirement
{
    Interact,
    Panel
}

public enum InteractionActionMode
{
    Instant,
    Hold
}

public readonly struct InteractionContext
{
    public readonly InteractionController Controller;
    public readonly Interactable Interactable;

    public InteractionContext(InteractionController controller, Interactable interactable)
    {
        Controller = controller;
        Interactable = interactable;
    }
}

public abstract class InteractionAction : ScriptableObject
{
    public virtual void Initialize(Interactable interactable) { }

    public virtual void Tick(Interactable interactable, float dtMs) { }

    public virtual void OnFocusEnter(InteractionContext context) { }

    public virtual void OnFocusExit(InteractionContext context) { }

    public virtual void OnStart(InteractionContext context) { }

    public virtual void OnProgress(InteractionContext context, float normalizedProgress) { }

    public virtual void OnComplete(InteractionContext context) { }

    public virtual void OnCancel(InteractionContext context) { }

    public virtual bool CanExecute(InteractionContext context) => true;

    public virtual InteractionAbilityRequirement RequiredAbility => InteractionAbilityRequirement.Interact;

    public virtual InteractionActionMode Mode => InteractionActionMode.Instant;

    public virtual bool LocksMovement => false;

    public virtual float GetRequiredHoldDurationMs(InteractionContext context)
    {
        return context.Interactable ? context.Interactable.holdDurationMs : 0f;
    }
}
