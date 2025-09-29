
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A general-purpose proxy to allow UnityEvents to call Animator methods that are normally inaccessible
/// or difficult to use. This script supports setting Bool, Trigger, Float, and Int parameters
/// and uses Animator.StringToHash for optimized performance.
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimatorEventProxy : MonoBehaviour
{
    [Header("Target Parameter Names")]
    [Tooltip("The name of the FLOAT parameter to control with the SetFloat method.")]
    public string floatParameterName;

    [Tooltip("The name of the INTEGER parameter to control with the SetInteger method.")]
    public string intParameterName;

    private Animator _animator;
    private readonly Dictionary<string, int> _parameterHashCache = new Dictionary<string, int>();

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Gets the integer hash for a given parameter name, using a cache for performance.
    /// </summary>
    private int GetParameterHash(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            Debug.LogWarning("Parameter name provided is empty. Cannot get hash.", this);
            return 0;
        }
        if (!_parameterHashCache.ContainsKey(parameterName))
        {
            _parameterHashCache[parameterName] = Animator.StringToHash(parameterName);
        }
        return _parameterHashCache[parameterName];
    }

    #region --- Public Methods for UnityEvents ---

    /// <summary>
    /// Sets a boolean parameter on the Animator to true. Call this from a UnityEvent.
    /// </summary>
    public void SetBoolTrue(string parameterName)
    {
        int hash = GetParameterHash(parameterName);
        if (hash != 0) _animator.SetBool(hash, true);
    }

    /// <summary>
    /// Sets a boolean parameter on the Animator to false. Call this from a UnityEvent.
    /// </summary>
    public void SetBoolFalse(string parameterName)
    {
        int hash = GetParameterHash(parameterName);
        if (hash != 0) _animator.SetBool(hash, false);
    }

    /// <summary>
    /// Fires a trigger on the Animator. Call this from a UnityEvent.
    /// </summary>
    public void SetTrigger(string parameterName)
    {
        int hash = GetParameterHash(parameterName);
        if (hash != 0) _animator.SetTrigger(hash);
    }

    /// <summary>
    /// Sets a float parameter on the Animator. The parameter name is specified in the inspector.
    /// </summary>
    public void SetFloat(float value)
    {
        int hash = GetParameterHash(floatParameterName);
        if (hash != 0) _animator.SetFloat(hash, value);
    }

    /// <summary>
    /// Sets an integer parameter on the Animator. The parameter name is specified in the inspector.
    /// </summary>
    public void SetInteger(int value)
    {
        int hash = GetParameterHash(intParameterName);
        if (hash != 0) _animator.SetInteger(hash, value);
    }

    #endregion
}
