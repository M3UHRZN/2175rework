using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Checkpoint Action")]
public class CheckpointAction : InteractActionBase
{
    [SerializeField] Transform checkpointTransform;
    [SerializeField] UnityEvent onCheckpoint;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (checkpointTransform)
        {
            InteractionStateRegistry.SetBool($"checkpoint:{checkpointTransform.GetInstanceID()}", true);
        }
        onCheckpoint?.Invoke();
    }
}
