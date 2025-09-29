
using UnityEngine;

/// <summary>
/// A component that acts as a trigger to change the state of a puzzle flag.
/// This should be placed on an Interactable GameObject.
/// </summary>
public class LogicTrigger : MonoBehaviour
{
    public enum TriggerAction
    {
        SetTrue,
        SetFalse,
        Toggle
    }

    [Tooltip("The name of the flag in the PuzzleStateManager to modify.")]
    public string flagName;

    [Tooltip("The action to perform on the flag when triggered.")]
    public TriggerAction action = TriggerAction.SetTrue;

    /// <summary>
    /// This public method should be hooked up to a UnityEvent, like from an Interactable's OnInteractComplete.
    /// </summary>
    public void Trigger()
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning("LogicTrigger has no flag name specified.", this);
            return;
        }

        switch (action)
        {
            case TriggerAction.SetTrue:
                PuzzleStateManager.Instance.SetFlagState(flagName, true);
                break;
            case TriggerAction.SetFalse:
                PuzzleStateManager.Instance.SetFlagState(flagName, false);
                break;
            case TriggerAction.Toggle:
                PuzzleStateManager.Instance.ToggleFlagState(flagName);
                break;
        }
    }
}
