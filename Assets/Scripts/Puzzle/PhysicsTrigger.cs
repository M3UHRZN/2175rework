
using UnityEngine;

/// <summary>
/// Triggers a puzzle flag when a physics object enters and exits a trigger collider.
/// Can be filtered by the actor type (Elior, Sim) via the InteractionController.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PhysicsTrigger : MonoBehaviour
{
    [Header("Puzzle Settings")]
    [Tooltip("The name of the flag in the PuzzleStateManager to modify.")]
    public string flagName;

    [Tooltip("Which character can activate this trigger?")]
    public InteractionActor requiredActor = InteractionActor.Any;

    private int _triggeringObjectCount = 0;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"The collider on {gameObject.name} must be set to 'Is Trigger' for PhysicsTrigger to work.", this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!CanTrigger(other.gameObject)) return;

        _triggeringObjectCount++;

        // Only set the flag on the first valid object entering
        if (_triggeringObjectCount == 1)
        {
            PuzzleStateManager.Instance.SetFlagState(flagName, true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!CanTrigger(other.gameObject)) return;

        _triggeringObjectCount--;

        // Only unset the flag when the last valid object leaves
        if (_triggeringObjectCount <= 0)
        {
            _triggeringObjectCount = 0; // Clamp to zero
            PuzzleStateManager.Instance.SetFlagState(flagName, false);
        }
    }

    private bool CanTrigger(GameObject obj)
    {
        if (requiredActor == InteractionActor.Any) return true;

        var controller = obj.GetComponent<InteractionController>();
        if (controller == null) return false;

        return controller.actor == requiredActor;
    }
}
