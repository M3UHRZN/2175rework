using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public enum InteractionActor
{
    Any,
    Elior,
    Sim
}

[System.Serializable]
public class InteractionControllerEvent : UnityEvent<InteractionController> { }

[System.Serializable]
public class InteractionProgressEvent : UnityEvent<float> { }

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public InteractionActor requiredActor = InteractionActor.Any;
    [FormerlySerializedAs("interactionType")]
    [SerializeField]
    InteractionType legacyInteractionType = InteractionType.Tap;
    [Tooltip("Maximum usable range for this interactable.")]
    public float range = 1.2f;
    public int priority = 0;
    public float holdDurationMs = 1000f;
    public float cooldownMs = 300f;
    public bool isLocked = false;
    [Tooltip("When true the interaction stays unlocked across sessions (handled externally).")]
    public bool persistent = false;

    [Header("Interaction Action")]
    public InteractionAction action;

    [Header("Events")]
    public InteractionControllerEvent OnFocusEnter;
    public InteractionControllerEvent OnFocusExit;
    public InteractionControllerEvent OnInteractStart;
    public InteractionProgressEvent OnInteractProgress;
    public InteractionControllerEvent OnInteractComplete;
    public InteractionControllerEvent OnInteractCancel;

    float cooldownRemaining;
    bool toggleState;
    InteractionAction runtimeAction;
    InteractionController activeController;

    public bool IsLocked => isLocked;
    public bool IsOnCooldown => cooldownRemaining > 0f;
    public bool ToggleState => toggleState;

    public void Tick(float dtMs)
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - dtMs);
        }

        var instance = GetActionInstance();
        instance?.Tick(this, dtMs);
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
        var context = CreateContext(controller);
        GetActionInstance()?.OnFocusEnter(context);
        OnFocusEnter?.Invoke(controller);
    }

    public void NotifyFocusExit(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnFocusExit(context);
        OnFocusExit?.Invoke(controller);
    }

    public void NotifyStart(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnStart(context);
        activeController = controller;
        OnInteractStart?.Invoke(controller);
    }

    public void NotifyProgress(float t)
    {
        var context = CreateContext(activeController);
        GetActionInstance()?.OnProgress(context, t);
        OnInteractProgress?.Invoke(t);
    }

    public void NotifyComplete(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnComplete(context);
        OnInteractComplete?.Invoke(controller);
        if (activeController == controller)
            activeController = null;
    }

    public void NotifyCancel(InteractionController controller)
    {
        var context = CreateContext(controller);
        GetActionInstance()?.OnCancel(context);
        OnInteractCancel?.Invoke(controller);
        if (activeController == controller)
            activeController = null;
    }

    public void ForceToggleState(bool value)
    {
        toggleState = value;
    }

    public InteractionActionMode ActionMode
    {
        get
        {
            var instance = GetActionInstance();
            return instance ? instance.Mode : InteractionActionMode.Instant;
        }
    }

    public InteractionAbilityRequirement AbilityRequirement
    {
        get
        {
            var instance = GetActionInstance();
            return instance ? instance.RequiredAbility : InteractionAbilityRequirement.Interact;
        }
    }

    public bool LocksMovement
    {
        get
        {
            var instance = GetActionInstance();
            return instance && instance.LocksMovement;
        }
    }

    public bool CanExecute(InteractionController controller)
    {
        var instance = GetActionInstance();
        if (!instance)
            return true;
        var context = CreateContext(controller);
        return instance.CanExecute(context);
    }

    public float GetRequiredHoldDurationMs(InteractionController controller)
    {
        var instance = GetActionInstance();
        if (!instance)
            return holdDurationMs;
        var context = CreateContext(controller);
        return instance.GetRequiredHoldDurationMs(context);
    }

    InteractionAction GetActionInstance()
    {
        if (!runtimeAction)
        {
            runtimeAction = CreateActionInstance();
            runtimeAction?.Initialize(this);
        }
        return runtimeAction;
    }

    InteractionAction CreateActionInstance()
    {
        if (action)
        {
            var instance = Instantiate(action);
            instance.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
            return instance;
        }

        switch (legacyInteractionType)
        {
            case InteractionType.Toggle:
                return CreateRuntimeAction<ToggleInteractionAction>();
            case InteractionType.Hold:
                return CreateRuntimeAction<HoldInteractionAction>();
            case InteractionType.Panel:
                return CreateRuntimeAction<PanelInteractionAction>();
            default:
                return CreateRuntimeAction<TapInteractionAction>();
        }
    }

    InteractionAction CreateRuntimeAction<T>() where T : InteractionAction
    {
        var created = ScriptableObject.CreateInstance<T>();
        created.hideFlags = HideFlags.DontUnloadUnusedAsset | HideFlags.HideAndDontSave;
        return created;
    }

    InteractionContext CreateContext(InteractionController controller)
    {
        return new InteractionContext(controller, this);
    }
}
