using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Checkpoint Action")]
public class CheckpointAction : InteractActionBase
{
    [System.Serializable]
    public class CheckpointEvent : UnityEvent<Vector3> { }

    [Header("Checkpoint")]
    [SerializeField] Transform spawnPoint;
    [SerializeField] CheckpointEvent onCheckpointReached = new CheckpointEvent();

    protected override void OnComplete(InteractionController controller)
    {
        Vector3 position = spawnPoint ? spawnPoint.position : transform.position;
        onCheckpointReached.Invoke(position);
    }
}
