using UnityEngine;
using UnityEngine.Events;

public enum InteractionActor
{
    Any,
    Elior,
    Sim
}

public enum InteractionType
{
    Tap,
    Hold,
    Toggle,
    Panel
}

[System.Serializable]
public class InteractionControllerEvent : UnityEvent<InteractionController> { }

[System.Serializable]
public class InteractionProgressEvent : UnityEvent<float> { }

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionActor requiredActor = InteractionActor.Any;
    public InteractionType interactionType = InteractionType.Tap;
    [Tooltip("Maximum usable range for this interactable.")]
    public float range = 1.2f;
    public int priority = 0;
    public float holdDurationMs = 1000f;
    public float cooldownMs = 300f;
    public bool isLocked = false;
    [Tooltip("When true the interaction stays unlocked across sessions (handled externally).")]
    public bool persistent = false;

    [Header("Events")]
    public InteractionControllerEvent OnFocusEnter;
    public InteractionControllerEvent OnFocusExit;
    public InteractionControllerEvent OnInteractStart;
    public InteractionProgressEvent OnInteractProgress;
    public InteractionControllerEvent OnInteractComplete;
    public InteractionControllerEvent OnInteractCancel;

    float cooldownRemaining;
    bool isActivated;

    public bool IsLocked => isLocked;
    public bool IsOnCooldown => cooldownRemaining > 0f;
    public bool IsActivated => isActivated;

    public void Tick(float dtMs)
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dtMs);
        }
    }

    public void BeginCooldown()
    {
        cooldownRemaining = Mathf.Max(cooldownRemaining, cooldownMs);
    }

    public bool AllowsActor(InteractionActor actor)
    {
        return requiredActor == InteractionActor.Any || requiredActor == actor;
    }

    public void NotifyFocusEnter(InteractionController controller)
    {
        OnFocusEnter?.Invoke(controller);
    }

    public void NotifyFocusExit(InteractionController controller)
    {
        OnFocusExit?.Invoke(controller);
    }

    public void NotifyStart(InteractionController controller)
    {
        OnInteractStart?.Invoke(controller);
    }

    public void NotifyProgress(float t)
    {
        OnInteractProgress?.Invoke(t);
    }

    public void NotifyComplete(InteractionController controller)
    {
        switch (interactionType)
        {
            case InteractionType.Toggle:
                isActivated = !isActivated;
                break;
            case InteractionType.Hold:
            case InteractionType.Panel:
                isActivated = true;
                break;
            default:
                isActivated = false;
                break;
        }
        OnInteractComplete?.Invoke(controller);
    }

    public void NotifyCancel(InteractionController controller)
    {
        if (interactionType == InteractionType.Hold || interactionType == InteractionType.Panel)
            isActivated = false;
        OnInteractCancel?.Invoke(controller);
    }

    public void ForceActivate(bool value)
    {
        isActivated = value;
    }
}
