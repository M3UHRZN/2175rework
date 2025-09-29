
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A component that checks a set of conditions from the PuzzleStateManager and fires events accordingly.
/// This can be used to open doors, activate platforms, etc.
/// </summary>
public class LogicGate : MonoBehaviour
{
    public enum GateType
    {
        // All conditions must be true.
        AND,
        // Any one of the conditions must be true.
        OR
    }

    [System.Serializable]
    public struct Condition
    {
        public string flagName;
        public bool requiredState;
    }

    [Tooltip("AND: All conditions must be met. OR: Any condition must be met.")]
    public GateType gateType = GateType.AND;

    [Tooltip("The list of puzzle flags and their required states for this gate to open.")]
    public List<Condition> conditions = new List<Condition>();

    [Header("Events")]
    public UnityEvent OnConditionsMet;
    public UnityEvent OnConditionsUnmet;

    private bool _areConditionsMet = false;

    void OnEnable()
    {
        foreach (var condition in conditions)
        {
            if (!string.IsNullOrEmpty(condition.flagName))
            {
                PuzzleStateManager.Instance.SubscribeToFlag(condition.flagName, OnFlagStateChanged);
            }
        }
        // Check initial state on enable
        CheckConditions();
    }

    void OnDisable()
    {
        foreach (var condition in conditions)
        {
            if (!string.IsNullOrEmpty(condition.flagName))
            {
                PuzzleStateManager.Instance.UnsubscribeFromFlag(condition.flagName, OnFlagStateChanged);
            }
        }
    }

    private void OnFlagStateChanged(bool newState)
    {
        CheckConditions();
    }

    /// <summary>
    /// Checks if the conditions for this gate are met and fires events on state change.
    /// </summary>
    [ContextMenu("Check Conditions Manually")]
    public void CheckConditions()
    {
        if (conditions.Count == 0) return;

        bool result = (gateType == GateType.AND);

        foreach (var condition in conditions)
        {
            bool flagState = PuzzleStateManager.Instance.GetFlagState(condition.flagName);
            bool conditionMet = (flagState == condition.requiredState);

            if (gateType == GateType.AND)
            {
                if (!conditionMet)
                {
                    result = false;
                    break;
                }
            }
            else // OR
            {
                if (conditionMet)
                {
                    result = true;
                    break;
                }
            }
        }

        // If the state hasn't changed, do nothing.
        if (result == _areConditionsMet) return;

        _areConditionsMet = result;

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
