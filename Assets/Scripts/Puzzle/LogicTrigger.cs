using UnityEngine;

namespace Puzzle
{
    /// <summary>
    /// A component that acts as a toggleable ILogicSource. Its state can be changed via the Trigger() method,
    /// which is typically hooked up to a UnityEvent (e.g., from an Interactable's OnComplete event).
    /// </summary>
    public class LogicTrigger : MonoBehaviour, ILogicSource
    {
        public enum TriggerAction
        {
            SetTrue,
            SetFalse,
            Toggle
        }

        [Tooltip("The action to perform on this trigger's state when Trigger() is called.")]
        public TriggerAction action = TriggerAction.Toggle;

        [Tooltip("The initial state of the trigger when the scene loads.")]
        [SerializeField] private bool _initialState = false;

        private bool _currentState;

        void Awake()
        {
            _currentState = _initialState;
        }

        /// <summary>
        /// Gets a value indicating whether this logic source is currently active.
        /// Implements the ILogicSource interface.
        /// </summary>
        public bool IsActive => _currentState;

        /// <summary>
        /// This public method should be hooked up to a UnityEvent, like from an Interactable's OnInteractComplete.
        /// It changes the internal state of this trigger.
        /// </summary>
        public void Trigger()
        {
            switch (action)
            {
                case TriggerAction.SetTrue:
                    _currentState = true;
                    break;
                case TriggerAction.SetFalse:
                    _currentState = false;
                    break;
                case TriggerAction.Toggle:
                    _currentState = !_currentState;
                    break;
            }
        }
    }
}