using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour
{
    public InteractionActor actor = InteractionActor.Any;
    public LayerMask interactionMask;
    [Tooltip("Extra distance before focus is released to avoid jitter.")]
    public float focusBuffer = 0.25f;

    InputAdapter input;
    AbilityLoadout loadout;
    Sensors2D sensors;

    Interactable focused;
    float focusedDistance;
    float holdElapsedMs;
    float holdRequiredMs;
    bool holding;
    bool movementLocked;
    float holdProgress;

    public Interactable Focused => focused;
    public float FocusedDistance => focusedDistance;
    public bool IsHolding => holding;
    public bool MovementLocked => movementLocked;
    public AbilitySnapshot CurrentAbilities => loadout ? loadout.ActiveSnapshot : AbilitySnapshot.AllEnabled;
    public int FocusedPriority => focused ? focused.priority : 0;
    public float HoldProgress => holdProgress;

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        loadout = GetComponent<AbilityLoadout>();
        sensors = GetComponent<Sensors2D>();

        if (interactionMask.value == 0)
        {
            if (sensors) interactionMask = sensors.interactMask;
        }
    }

    public void Tick(float dt)
    {
        float dtMs = dt * 1000f;

        // Tick the cooldown of the currently focused interactable if it exists.
        // Note: The responsibility of ticking all potential interactables is now in Sensors2D.
        if (focused)
        {
            focused.Tick(dtMs);
        }

        UpdateFocus(dtMs);
        UpdateInteraction(dtMs);
    }

    void UpdateFocus(float dtMs)
    {
        Vector2 origin = transform.position;
        Interactable best = sensors ? sensors.BestInteractable : null;

        // We also need to tick the best candidate if it's not the one in focus,
        // so its cooldown state is up-to-date for the AllowsActor check.
        if (best && best != focused) 
        {
            best.Tick(dtMs);
        }

        bool keepCurrent = false;
        if (focused)
        {
            float dist = Vector2.Distance(origin, focused.transform.position);
            bool isStillValid = AllowsActor(focused) && dist <= focused.range + focusBuffer;

            if (isStillValid)
            {
                if (best == null || best == focused)
                {
                    keepCurrent = true;
                }
                else
                {
                    if (best.priority > focused.priority)
                    {
                        keepCurrent = false;
                    }
                    else if (best.priority == focused.priority && Vector2.Distance(origin, best.transform.position) < dist - 0.05f)
                    {
                        keepCurrent = false;
                    }
                    else
                    {
                        keepCurrent = true;
                    }
                }
            }
        }

        if (!keepCurrent)
        {
            if (focused && focused != best)
            {
                if (holding)
                {
                    CancelInteraction();
                }
                focused.NotifyFocusExit(this);
            }
            focused = best;
            if (focused)
            {
                focusedDistance = Vector2.Distance(origin, focused.transform.position);
                focused.NotifyFocusEnter(this);
            }
            else
            {
                focusedDistance = 0f;
            }
        }
        else if (focused)
        {
            focusedDistance = Vector2.Distance(origin, focused.transform.position);
        }

        if (focused && (!AllowsActor(focused) || Vector2.Distance(origin, focused.transform.position) > focused.range + focusBuffer))
        {
            if (holding)
            {
                CancelInteraction();
            }
            focused.NotifyFocusExit(this);
            focused = null;
            focusedDistance = 0f;
            holdProgress = 0f;
        }
        else if (!focused)
        {
            holdProgress = 0f;
        }
    }

    void UpdateInteraction(float dtMs)
    {
        if (!focused)
        {
            if (holding)
            {
                CancelInteraction();
            }
            return;
        }

        var abilities = loadout ? loadout.ActiveSnapshot : AbilitySnapshot.AllEnabled;
        if (!HasAbilityFor(focused.interactionType, abilities))
        {
            if (holding)
                CancelInteraction();
            return;
        }

        if (input.InteractPressed)
        {
            StartInteraction();
        }

        if (holding)
        {
            if (input.InteractHeld)
            {
                holdElapsedMs += dtMs;
                holdProgress = Mathf.Clamp01(holdRequiredMs <= 0f ? 1f : holdElapsedMs / Mathf.Max(1f, holdRequiredMs));
                focused.NotifyProgress(holdProgress);
                if (holdElapsedMs >= holdRequiredMs)
                {
                    CompleteInteraction();
                }
            }
            else if (input.InteractReleased)
            {
                CancelInteraction();
            }
        }
    }

    bool AllowsActor(Interactable interactable)
    {
        if (!interactable || interactable.IsLocked || interactable.IsOnCooldown)
            return false;
        return interactable.AllowsActor(actor);
    }

    bool HasAbilityFor(InteractionType type, AbilitySnapshot abilities)
    {
        switch (type)
        {
            case InteractionType.Panel:
                return abilities.canUsePanels;
            default:
                return abilities.canInteract;
        }
    }

    void StartInteraction()
    {
        if (!focused)
            return;

        switch (focused.interactionType)
        {
            case InteractionType.Tap:
                focused.NotifyStart(this);
                focused.NotifyComplete(this);
                focused.BeginCooldown();
                break;
            case InteractionType.Toggle:
                focused.NotifyStart(this);
                focused.NotifyComplete(this);
                focused.BeginCooldown();
                break;
            case InteractionType.Hold:
            case InteractionType.Panel:
                holding = true;
                holdElapsedMs = 0f;
                holdRequiredMs = Mathf.Max(1f, focused.holdDurationMs);
                movementLocked = focused.interactionType == InteractionType.Panel;
                focused.NotifyStart(this);
                holdProgress = 0f;
                focused.NotifyProgress(0f);
                break;
        }
    }

    void CompleteInteraction()
    {
        if (!focused)
            return;

        holding = false;
        movementLocked = false;
        holdElapsedMs = 0f;
        holdProgress = 0f;
        focused.NotifyProgress(1f);
        focused.NotifyComplete(this);
        focused.BeginCooldown();
    }

    void CancelInteraction()
    {
        var interactableToCancel = focused;

        holding = false;
        movementLocked = false;

        if (interactableToCancel == null)
        {
            return;
        }

        holdElapsedMs = 0f;
        holdProgress = 0f;
        interactableToCancel.NotifyCancel(this);
    }

    public void ForceCancelInteraction()
    {
        CancelInteraction();
    }
}
