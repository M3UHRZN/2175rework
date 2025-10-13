
using UnityEngine;

/// <summary>
/// Syncs a boolean parameter on an Animator with the state of a specific flag
/// in the PuzzleStateManager. Useful for animating objects based on puzzle progress.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PuzzleStateAnimatorSync : MonoBehaviour
{
    [Tooltip("The name of the flag in the PuzzleStateManager to listen to.")]
    public string flagName;

    [Tooltip("The name of the boolean parameter in the Animator to sync with.")]
    public string boolParameterName = "isPressed"; // Default to isPressed, but can be anything

    private Animator _animator;
    private int _animatorParamHash;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        if (!string.IsNullOrEmpty(boolParameterName))
        {
            _animatorParamHash = Animator.StringToHash(boolParameterName);
        }
    }

    void OnEnable()
    {
        if (string.IsNullOrEmpty(flagName))
        {
            Debug.LogWarning($"PuzzleStateAnimatorSync on {gameObject.name} has no flag name specified.", this);
            return;
        }

        // Subscribe to flag changes and set initial state
        PuzzleStateManager.Instance.SubscribeToFlag(flagName, OnFlagStateChanged);
        OnFlagStateChanged(PuzzleStateManager.Instance.GetFlagState(flagName));
    }

    void OnDisable()
    {
        if (string.IsNullOrEmpty(flagName)) return;
        PuzzleStateManager.Instance.UnsubscribeFromFlag(flagName, OnFlagStateChanged);
    }

    private void OnFlagStateChanged(bool newState)
    {
        if (_animator == null || _animatorParamHash == 0) return;
        _animator.SetBool(_animatorParamHash, newState);
    }
}
