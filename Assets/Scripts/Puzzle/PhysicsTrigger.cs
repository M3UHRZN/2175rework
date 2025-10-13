using UnityEngine;

namespace Puzzle
{
    /// <summary>
    /// An ILogicSource that becomes active when a specified physics object enters its trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PhysicsTrigger : MonoBehaviour, ILogicSource
    {
        [Tooltip("Which character can activate this trigger?")]
        public InteractionActor requiredActor = InteractionActor.Any;

        private int _triggeringObjectCount = 0;

        /// <summary>
        /// Gets a value indicating whether a valid object is currently inside the trigger.
        /// Implements the ILogicSource interface.
        /// </summary>
        public bool IsActive => _triggeringObjectCount > 0;

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
            if (CanTrigger(other.gameObject))
            {
                _triggeringObjectCount++;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (CanTrigger(other.gameObject))
            {
                _triggeringObjectCount--;
                if (_triggeringObjectCount < 0)
                {
                    _triggeringObjectCount = 0;
                }
            }
        }

        private bool CanTrigger(GameObject obj)
        {
            var controller = obj.GetComponent<InteractionController>();
            if (controller == null) return false;

            if (requiredActor == InteractionActor.Any)
            {
                // If 'Any' is selected, we still want to ensure it's a character, not just any physics object.
                return controller.actor == InteractionActor.Elior || controller.actor == InteractionActor.Sim;
            }
            
            return controller.actor == requiredActor;
        }
    }
}