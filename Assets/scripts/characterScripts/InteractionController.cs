using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour
{
    public InteractionActor actor = InteractionActor.Any;
    public LayerMask interactionMask;
    [Tooltip("Scan radius used to discover interactables.")]
    public float scanRadius = 1.6f;
    [Tooltip("Extra distance before focus is released to avoid jitter.")]
    public float focusBuffer = 0.25f;

    InputAdapter input;
    AbilityLoadout loadout;

    readonly Collider2D[] overlapResults = new Collider2D[16];
    readonly List<Interactable> candidates = new List<Interactable>(16);

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

        if (interactionMask.value == 0)
        {
            var sensors = GetComponent<Sensors2D>();
            if (sensors) interactionMask = sensors.interactMask;
        }
    }

    public void Tick(float dt)
    {
        float dtMs = dt * 1000f;
        UpdateFocus(dtMs);
        UpdateInteraction(dtMs);
    }

    void UpdateFocus(float dtMs)
    {
        Vector2 origin = transform.position;
        candidates.Clear();

        int count = Physics2D.OverlapCircleNonAlloc(origin, scanRadius, overlapResults, interactionMask);
        for (int i = 0; i < count; i++)
        {
            var collider = overlapResults[i];
            if (!collider) continue;
            var interactable = collider.GetComponent<Interactable>() ?? collider.GetComponentInParent<Interactable>();
            if (!interactable) continue;
            if (!candidates.Contains(interactable))
            {
                interactable.Tick(dtMs);
                candidates.Add(interactable);
            }
        }

        Interactable best = null;
        float bestDist = float.MaxValue;
        int bestPriority = int.MinValue;
        foreach (var c in candidates)
        {
            if (!AllowsActor(c))
                continue;
            float dist = Vector2.Distance(origin, c.transform.position);
            if (dist > c.range)
                continue;

            if (c.priority > bestPriority || (c.priority == bestPriority && dist < bestDist))
            {
                bestPriority = c.priority;
                bestDist = dist;
                best = c;
            }
        }

        bool keepCurrent = false;
        if (focused)
        {
            float dist = Vector2.Distance(origin, focused.transform.position);
            if (AllowsActor(focused) && dist <= focused.range + focusBuffer)
            {
                if (best && best != focused)
                {
                    if (best.priority > focused.priority)
                        keepCurrent = false;
                    else if (best.priority == focused.priority && bestDist + 0.05f < dist)
                        keepCurrent = false;
                    else
                        keepCurrent = true;
                }
                else
                {
                    keepCurrent = true;
                }
            }
        }

        if (!keepCurrent)
        {
            if (focused && focused != best)
            {
                focused.NotifyFocusExit(this);
            }
            focused = best;
            if (focused)
            {
                focusedDistance = bestDist;
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

        if (focused && (!AllowsActor(focused) || !focused.CanBeFocusedBy(this) || Vector2.Distance(origin, focused.transform.position) > focused.range + focusBuffer))
        {
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
        if (!HasAbilityFor(focused, abilities))
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
        if (!interactable.AllowsActor(actor))
            return false;
        return interactable.CanBeFocusedBy(this);
    }

    bool HasAbilityFor(Interactable interactable, AbilitySnapshot abilities)
    {
        if (!interactable)
            return false;

        switch (interactable.AbilityRequirement)
        {
            case InteractionAbilityRequirement.Panel:
                return abilities.canUsePanels;
            default:
                return abilities.canInteract;
        }
    }

    void StartInteraction()
    {
        if (!focused)
            return;

        if (!focused.CanExecute(this))
            return;

        focused.NotifyStart(this);

        if (focused.ActionMode == InteractionActionMode.Hold)
        {
            holding = true;
            holdElapsedMs = 0f;
            holdRequiredMs = Mathf.Max(1f, focused.GetRequiredHoldDurationMs(this));
            movementLocked = focused.LocksMovement;
            holdProgress = 0f;
            focused.NotifyProgress(0f);
        }
        else
        {
            focused.NotifyProgress(1f);
            focused.NotifyComplete(this);
            focused.BeginCooldown();
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
        if (!focused)
        {
            holding = false;
            movementLocked = false;
            return;
        }

        holding = false;
        movementLocked = false;
        holdElapsedMs = 0f;
        holdProgress = 0f;
        focused.NotifyCancel(this);
    }

    public void ForceCancelInteraction()
    {
        CancelInteraction();
    }
}
