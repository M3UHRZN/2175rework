
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A singleton manager for the state of all puzzle elements in a scene.
/// It holds a dictionary of flags (string) and their boolean state.
/// </summary>
public class PuzzleStateManager : MonoBehaviour
{
    #region Singleton
    private static PuzzleStateManager _instance;
    public static PuzzleStateManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<PuzzleStateManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("PuzzleStateManager");
                    _instance = go.AddComponent<PuzzleStateManager>();
                }
            }
            return _instance;
        }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }
    #endregion

    private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();
    private readonly Dictionary<string, UnityEvent<bool>> _flagEvents = new Dictionary<string, UnityEvent<bool>>();

    /// <summary>
    /// Gets the current state of a flag. Returns false if the flag doesn't exist.
    /// </summary>
    public bool GetFlagState(string flagName)
    {
        _flags.TryGetValue(flagName, out bool value);
        return value;
    }

    /// <summary>
    /// Sets the state of a flag and invokes its corresponding event.
    /// </summary>
    public void SetFlagState(string flagName, bool value)
    {
        bool oldValue = GetFlagState(flagName);
        if (oldValue == value) return;

        _flags[flagName] = value;
        Debug.Log($"[PuzzleStateManager] Flag '{flagName}' set to {value}");

        if (_flagEvents.TryGetValue(flagName, out var flagEvent))
        {
            flagEvent.Invoke(value);
        }
    }

    /// <summary>
    /// Toggles the current state of a flag.
    /// </summary>
    public void ToggleFlagState(string flagName)
    {
        SetFlagState(flagName, !GetFlagState(flagName));
    }

    /// <summary>
    /// Subscribes a listener to a specific flag's state change event.
    /// </summary>
    public void SubscribeToFlag(string flagName, UnityAction<bool> listener)
    {
        if (!_flagEvents.ContainsKey(flagName))
        {
            _flagEvents[flagName] = new UnityEvent<bool>();
        }
        _flagEvents[flagName].AddListener(listener);
    }

    /// <summary>
    /// Unsubscribes a listener from a specific flag's state change event.
    /// </summary>
    public void UnsubscribeFromFlag(string flagName, UnityAction<bool> listener)
    {
        if (_flagEvents.TryGetValue(flagName, out var flagEvent))
        {
            flagEvent.RemoveListener(listener);
        }
    }
}
