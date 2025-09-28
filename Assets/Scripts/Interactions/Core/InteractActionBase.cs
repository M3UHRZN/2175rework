using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Interactable))]
public abstract class InteractActionBase : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    Interactable target;

    [Header("Event Listening")]
    [SerializeField]
    bool listenOnFocusEnter;
    [SerializeField]
    bool listenOnFocusExit;
    [SerializeField]
    bool listenOnStart;
    [SerializeField]
    bool listenOnProgress;
    [SerializeField]
    bool listenOnComplete = true;
    [SerializeField]
    bool listenOnCancel;

    [Header("Actor Filter")]
    [SerializeField]
    bool restrictActor;
    [SerializeField]
    InteractionActor overrideActorFilter = InteractionActor.Any;

    [Header("Cooldown")]
    [Tooltip("Optional local cooldown applied per action in milliseconds.")]
    [Min(0f)]
    [SerializeField]
    float localCooldownMs;

    InteractionController activeController;
    double lastEventTime = double.NegativeInfinity;

    UnityAction<InteractionController> onFocusEnterHandler;
    UnityAction<InteractionController> onFocusExitHandler;
    UnityAction<InteractionController> onStartHandler;
    UnityAction<float> onProgressHandler;
    UnityAction<InteractionController> onCompleteHandler;
    UnityAction<InteractionController> onCancelHandler;

    protected Interactable Target => target;
    protected InteractionController ActiveController => activeController;
    protected bool IsCoolingDown => localCooldownMs > 0f && (Time.unscaledTimeAsDouble - lastEventTime) * 1000d < localCooldownMs;

    protected virtual void Reset()
    {
        target = GetComponent<Interactable>();
        restrictActor = false;
        listenOnFocusEnter = false;
        listenOnFocusExit = false;
        listenOnStart = false;
        listenOnProgress = false;
        listenOnCancel = false;
        listenOnComplete = true;
    }

    protected virtual void Awake()
    {
        CacheHandlers();
        if (!target)
            target = GetComponent<Interactable>();
    }

    protected virtual void OnEnable()
    {
        if (!target)
            target = GetComponent<Interactable>();
        if (!target)
            return;

        CacheHandlers();
        EnsureEvents();

        if (listenOnFocusEnter)
            target.OnFocusEnter.AddListener(onFocusEnterHandler);
        if (listenOnFocusExit)
            target.OnFocusExit.AddListener(onFocusExitHandler);
        if (listenOnStart)
            target.OnInteractStart.AddListener(onStartHandler);
        if (listenOnProgress)
            target.OnInteractProgress.AddListener(onProgressHandler);
        if (listenOnComplete)
            target.OnInteractComplete.AddListener(onCompleteHandler);
        if (listenOnCancel)
            target.OnInteractCancel.AddListener(onCancelHandler);
    }

    protected virtual void OnDisable()
    {
        if (!target)
            return;

        if (listenOnFocusEnter)
            target.OnFocusEnter.RemoveListener(onFocusEnterHandler);
        if (listenOnFocusExit)
            target.OnFocusExit.RemoveListener(onFocusExitHandler);
        if (listenOnStart)
            target.OnInteractStart.RemoveListener(onStartHandler);
        if (listenOnProgress)
            target.OnInteractProgress.RemoveListener(onProgressHandler);
        if (listenOnComplete)
            target.OnInteractComplete.RemoveListener(onCompleteHandler);
        if (listenOnCancel)
            target.OnInteractCancel.RemoveListener(onCancelHandler);
    }

    protected void SetDefaultListeners(bool focusEnter, bool focusExit, bool start, bool progress, bool complete, bool cancel)
    {
        listenOnFocusEnter = focusEnter;
        listenOnFocusExit = focusExit;
        listenOnStart = start;
        listenOnProgress = progress;
        listenOnComplete = complete;
        listenOnCancel = cancel;
    }

    protected void SetDefaultListeners(bool start, bool progress, bool complete, bool cancel)
    {
        SetDefaultListeners(false, false, start, progress, complete, cancel);
    }

    void CacheHandlers()
    {
        if (onFocusEnterHandler == null)
            onFocusEnterHandler = HandleFocusEnter;
        if (onFocusExitHandler == null)
            onFocusExitHandler = HandleFocusExit;
        if (onStartHandler == null)
            onStartHandler = HandleStart;
        if (onProgressHandler == null)
            onProgressHandler = HandleProgress;
        if (onCompleteHandler == null)
            onCompleteHandler = HandleComplete;
        if (onCancelHandler == null)
            onCancelHandler = HandleCancel;
    }

    void EnsureEvents()
    {
        if (target.OnFocusEnter == null)
            target.OnFocusEnter = new InteractionControllerEvent();
        if (target.OnFocusExit == null)
            target.OnFocusExit = new InteractionControllerEvent();
        if (target.OnInteractStart == null)
            target.OnInteractStart = new InteractionControllerEvent();
        if (target.OnInteractProgress == null)
            target.OnInteractProgress = new InteractionProgressEvent();
        if (target.OnInteractComplete == null)
            target.OnInteractComplete = new InteractionControllerEvent();
        if (target.OnInteractCancel == null)
            target.OnInteractCancel = new InteractionControllerEvent();
    }

    protected bool AllowsActor(InteractionController controller)
    {
        if (!restrictActor || overrideActorFilter == InteractionActor.Any)
            return true;
        if (!controller)
            return false;
        return controller.actor == overrideActorFilter;
    }

    bool ConsumeCooldown()
    {
        if (localCooldownMs <= 0f)
            return true;

        double time = Time.unscaledTimeAsDouble;
        if ((time - lastEventTime) * 1000d < localCooldownMs)
            return false;

        lastEventTime = time;
        return true;
    }

    void HandleFocusEnter(InteractionController controller)
    {
        if (!isActiveAndEnabled || !target)
            return;
        if (controller && !AllowsActor(controller))
            return;
        activeController = controller;
        OnFocusEnter(controller);
    }

    void HandleFocusExit(InteractionController controller)
    {
        if (!isActiveAndEnabled || !target)
            return;
        OnFocusExit(controller);
        if (activeController == controller)
            activeController = null;
    }

    void HandleStart(InteractionController controller)
    {
        if (!isActiveAndEnabled || !target || !controller)
            return;
        if (!AllowsActor(controller))
            return;
        if (!ConsumeCooldown())
            return;

        activeController = controller;
        OnStart(controller);
    }

    void HandleProgress(float value)
    {
        if (!isActiveAndEnabled || !target)
            return;
        OnProgress(Mathf.Clamp01(value));
    }

    void HandleComplete(InteractionController controller)
    {
        if (!isActiveAndEnabled || !target)
            return;
        if (controller && !AllowsActor(controller))
            return;
        if (!ConsumeCooldown())
            return;

        activeController = controller;
        OnComplete(controller);
    }

    void HandleCancel(InteractionController controller)
    {
        if (!isActiveAndEnabled || !target)
            return;
        if (controller && !AllowsActor(controller))
            return;
        OnCancel(controller);
        if (activeController == controller)
            activeController = null;
    }

    protected virtual void OnFocusEnter(InteractionController controller) { }
    protected virtual void OnFocusExit(InteractionController controller) { }
    protected virtual void OnStart(InteractionController controller) { }
    protected virtual void OnProgress(float value) { }
    protected virtual void OnComplete(InteractionController controller) { }
    protected virtual void OnCancel(InteractionController controller) { }
}
