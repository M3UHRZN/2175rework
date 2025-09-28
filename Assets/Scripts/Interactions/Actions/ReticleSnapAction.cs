using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Reticle Snap Action")]
public class ReticleSnapAction : InteractActionBase
{
    [System.Serializable]
    public class ReticleLockEvent : UnityEvent<Transform> { }

    [Header("Reticle Snap")]
    [SerializeField] Transform snapTarget;
    [SerializeField] ReticleLockEvent onLock = new ReticleLockEvent();
    [SerializeField] UnityEvent onRelease = new UnityEvent();

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        onLock.Invoke(snapTarget ? snapTarget : transform);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onRelease.Invoke();
    }

    protected override void OnCancel(InteractionController controller)
    {
        onRelease.Invoke();
    }
}
