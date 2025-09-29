using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour
{
    public InteractionActor actor = InteractionActor.Any;
    [Tooltip("Taranacak maksimum yarıçap.")]
    public float scanRadius = 2f;
    [Tooltip("Odak kaybını geciktirmek için ek tolerans (metre).")]
    public float focusBuffer = 0.25f;

    InputAdapter input;
    AbilityLoadout loadout;

    readonly List<Interactable> adaylar = new List<Interactable>(32);

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
        adaylar.Clear();
        Interactable.ToplananEtkilesimleriDoldur(adaylar);

        Interactable best = null;
        float bestDist = float.MaxValue;
        int bestPriority = int.MinValue;

        for (int i = 0; i < adaylar.Count; i++)
        {
            var interactable = adaylar[i];
            if (!interactable)
                continue;

            if (!AllowsActor(interactable))
                continue;

            float dist = Vector2.Distance(origin, interactable.transform.position);
            if (dist > scanRadius)
                continue;
            if (dist > interactable.range)
                continue;

            if (interactable.priority > bestPriority || (interactable.priority == bestPriority && dist < bestDist))
            {
                bestPriority = interactable.priority;
                bestDist = dist;
                best = interactable;
            }
        }

        bool keepCurrent = false;
        if (focused)
        {
            float dist = Vector2.Distance(origin, focused.transform.position);
            bool icerde = dist <= Mathf.Max(focused.range, scanRadius) + focusBuffer;
            if (!AllowsActor(focused) || !icerde)
            {
                CancelAndClearFocus();
            }
            else if (best && best != focused)
            {
                if (best.priority > focused.priority)
                {
                    keepCurrent = false;
                }
                else if (best.priority == focused.priority && bestDist + 0.05f < dist)
                {
                    keepCurrent = false;
                }
                else
                {
                    keepCurrent = true;
                }
            }
            else
            {
                keepCurrent = true;
            }

            if (keepCurrent)
            {
                focusedDistance = dist;
            }
        }

        if (!keepCurrent)
        {
            if (focused && focused != best)
            {
                var previous = focused;
                if (holding)
                    CancelInteraction(previous);
                previous.NotifyFocusExit(this);
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

        if (focused && !AllowsActor(focused))
        {
            CancelAndClearFocus();
        }
        else if (!focused)
        {
            holdProgress = 0f;
        }
    }

    void CancelAndClearFocus()
    {
        if (!focused)
            return;

        var previous = focused;
        if (holding)
            CancelInteraction(previous);
        previous.NotifyFocusExit(this);
        focused = null;
        focusedDistance = 0f;
        holdProgress = 0f;
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

        if (input && input.InteractPressed)
        {
            StartInteraction();
        }

        if (holding)
        {
            if (input && input.InteractHeld)
            {
                holdElapsedMs += dtMs;
                holdProgress = Mathf.Clamp01(holdRequiredMs <= 0f ? 1f : holdElapsedMs / Mathf.Max(1f, holdRequiredMs));
                focused.NotifyProgress(holdProgress);
                if (holdElapsedMs >= holdRequiredMs)
                {
                    CompleteInteraction();
                }
            }
            else if (input && input.InteractReleased)
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
        if (!interactable.CanBeFocusedBy(this))
            return false;
        return true;
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
        CancelInteraction(focused);
    }

    void CancelInteraction(Interactable target)
    {
        holding = false;
        movementLocked = false;
        holdElapsedMs = 0f;
        holdRequiredMs = 0f;
        holdProgress = 0f;

        if (!target)
            return;

        target.NotifyCancel(this);
    }

    public void ForceCancelInteraction()
    {
        CancelInteraction(focused);
    }
}
