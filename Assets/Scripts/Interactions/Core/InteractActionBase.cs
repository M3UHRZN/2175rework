using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Core
{
    public abstract class InteractActionBase : MonoBehaviour
    {
        [Header("Interaction Target")]
        [Tooltip("Interactable component that raises the events this action listens to.")]
        [SerializeField] Interactable target;

        [Header("Event Binding")]
        [SerializeField] bool listenOnStart = true;
        [SerializeField] bool listenOnProgress = false;
        [SerializeField] bool listenOnComplete = true;
        [SerializeField] bool listenOnCancel = false;

        [Header("Actor Filter")]
        [Tooltip("Override interaction actor required for this action to respond.")]
        [SerializeField] bool overrideActorFilter = false;
        [SerializeField] InteractionActor actorFilter = InteractionActor.Any;

        [Header("Timing")]
        [Tooltip("Optional local cooldown in milliseconds before the action can fire again.")]
        [SerializeField] float localCooldownMs = 0f;

        readonly UnityAction<InteractionController> startHandler;
        readonly UnityAction<float> progressHandler;
        readonly UnityAction<InteractionController> completeHandler;
        readonly UnityAction<InteractionController> cancelHandler;

        float nextAllowedTimeMs;

        protected InteractActionBase()
        {
            startHandler = HandleStart;
            progressHandler = HandleProgress;
            completeHandler = HandleComplete;
            cancelHandler = HandleCancel;
        }

        protected virtual void Awake()
        {
            if (!target)
            {
                target = GetComponent<Interactable>();
            }
        }

        protected virtual void OnValidate()
        {
            if (!target)
            {
                target = GetComponent<Interactable>();
            }
        }

        protected virtual void OnEnable()
        {
            if (!target)
                target = GetComponent<Interactable>();

            if (!target)
                return;

            if (listenOnStart)
                target.OnInteractStart.AddListener(startHandler);
            if (listenOnProgress)
                target.OnInteractProgress.AddListener(progressHandler);
            if (listenOnComplete)
                target.OnInteractComplete.AddListener(completeHandler);
            if (listenOnCancel)
                target.OnInteractCancel.AddListener(cancelHandler);
        }

        protected virtual void OnDisable()
        {
            if (!target)
                return;

            if (listenOnStart)
                target.OnInteractStart.RemoveListener(startHandler);
            if (listenOnProgress)
                target.OnInteractProgress.RemoveListener(progressHandler);
            if (listenOnComplete)
                target.OnInteractComplete.RemoveListener(completeHandler);
            if (listenOnCancel)
                target.OnInteractCancel.RemoveListener(cancelHandler);
        }

        protected void SetEventListening(bool onStart, bool onProgress, bool onComplete, bool onCancel)
        {
            listenOnStart = onStart;
            listenOnProgress = onProgress;
            listenOnComplete = onComplete;
            listenOnCancel = onCancel;
        }

        bool CanProcess(InteractionController controller)
        {
            if (!enabled || !target)
                return false;

            if (overrideActorFilter && actorFilter != InteractionActor.Any)
            {
                if (!controller || controller.actor != actorFilter)
                    return false;
            }

            if (localCooldownMs > 0f)
            {
                float currentMs = Time.unscaledTime * 1000f;
                if (currentMs < nextAllowedTimeMs)
                    return false;
            }

            return true;
        }

        void StampCooldown()
        {
            if (localCooldownMs > 0f)
            {
                nextAllowedTimeMs = Time.unscaledTime * 1000f + localCooldownMs;
            }
        }

        void HandleStart(InteractionController controller)
        {
            if (!CanProcess(controller))
                return;
            OnStart(controller);
            StampCooldown();
        }

        void HandleProgress(float value)
        {
            if (!enabled || !target)
                return;
            OnProgress(value);
        }

        void HandleComplete(InteractionController controller)
        {
            if (!CanProcess(controller))
                return;
            OnComplete(controller);
            StampCooldown();
        }

        void HandleCancel(InteractionController controller)
        {
            if (!CanProcess(controller))
                return;
            OnCancel(controller);
        }

        protected virtual void OnStart(InteractionController controller) { }
        protected virtual void OnProgress(float value) { }
        protected virtual void OnComplete(InteractionController controller) { }
        protected virtual void OnCancel(InteractionController controller) { }

        protected Interactable Target => target;
        protected bool HasActorOverride => overrideActorFilter && actorFilter != InteractionActor.Any;
        protected InteractionActor ActorFilter => actorFilter;
    }
}
