using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Puzzle
{
    /// <summary>
    /// An ILogicSource that evaluates a set of other ILogicSource inputs based on a specific rule (AND/OR)
    /// and fires events when its state changes. It can also be used as an input for other LogicGates.
    /// </summary>
    public class LogicGate : MonoBehaviour, ILogicSource
    {
        public enum GateType
        {
            AND, // All inputs must be in the required state.
            OR   // Any one of the inputs must be in the required state.
        }

        [System.Serializable]
        public struct LogicInput
        {
            [Tooltip("The GameObject containing a component that acts as a logic source (e.g., LogicTrigger, LogicGate).")]
            public GameObject sourceGameObject;

            [Tooltip("The state required from the source for this condition to be met.")]
            public bool requiredState;

            // Private cache for the ILogicSource interface to avoid repeated GetComponent calls.
            private ILogicSource _sourceInterface;

            /// <summary>
            /// Gets the ILogicSource interface from the assigned GameObject.
            /// </summary>
            public ILogicSource Source
            {
                get
                {
                    if (_sourceInterface == null && sourceGameObject != null)
                    {
                        // Important: Use GetComponentInParent or other variations if the source might be on a different GameObject.
                        _sourceInterface = sourceGameObject.GetComponent<ILogicSource>();
                        if (_sourceInterface == null)
                        {
                            Debug.LogError($"The GameObject '{sourceGameObject.name}' does not have a component that implements ILogicSource.", sourceGameObject);
                        }
                    }
                    return _sourceInterface;
                }
            }
        }

        [Tooltip("AND: All inputs must be met. OR: Any input must be met.")]
        public GateType gateType = GateType.AND;

        [Tooltip("The list of logic sources that will be evaluated by this gate.")]
        public List<LogicInput> inputs = new List<LogicInput>();

        [Header("Events")]
        public UnityEvent OnConditionsMet;
        public UnityEvent OnConditionsUnmet;

        private bool _areConditionsMet = false;

        /// <summary>
        /// Gets a value indicating whether the gate's conditions are currently met.
        /// Implements the ILogicSource interface, allowing this gate to be an input for another gate.
        /// </summary>
        public bool IsActive => _areConditionsMet;

        void Start()
        {
            // Check initial state on start.
            CheckConditions();
        }

        void Update()
        {
            // Continuously check conditions every frame.
            CheckConditions();
        }

        /// <summary>
        /// Checks if the conditions for this gate are met and fires events on state change.
        /// </summary>
        [ContextMenu("Check Conditions Manually")]
        public void CheckConditions()
        { 
            if (inputs == null || inputs.Count == 0) return;

            bool overallResult;
            if (gateType == GateType.AND)
            {
                overallResult = true; // Assume true and look for a failure.
                foreach (var input in inputs)
                {
                    if (input.Source == null || input.Source.IsActive != input.requiredState)
                    {
                        overallResult = false;
                        break;
                    }
                }
            }
            else // OR
            {
                overallResult = false; // Assume false and look for a success.
                foreach (var input in inputs)
                {
                    if (input.Source != null && input.Source.IsActive == input.requiredState)
                    {
                        overallResult = true;
                        break;
                    }
                }
            }

            // If the state hasn't changed, do nothing to avoid firing events repeatedly.
            if (overallResult == _areConditionsMet) return;

            _areConditionsMet = overallResult;

            if (_areConditionsMet)
            {
                Debug.Log($"[LogicGate] Conditions met for '{gameObject.name}'. Firing OnConditionsMet.", this);
                OnConditionsMet?.Invoke();
            }
            else
            {
                Debug.Log($"[LogicGate] Conditions no longer met for '{gameObject.name}'. Firing OnConditionsUnmet.", this);
                OnConditionsUnmet?.Invoke();
            }
        }
    }
}