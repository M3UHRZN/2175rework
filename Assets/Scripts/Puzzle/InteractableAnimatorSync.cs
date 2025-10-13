
using UnityEngine;

/// <summary>
/// Continuously syncs the boolean state of an Interactable component
/// with a boolean parameter on an Animator component on the same GameObject.
/// </summary>
[RequireComponent(typeof(Animator), typeof(Interactable))]
public class InteractableAnimatorSync : MonoBehaviour
{
    [Tooltip("The name of the boolean parameter in the Animator to sync with.")]
    public string boolParameterName = "isActivated";

    private Animator _animator;
    private Interactable _interactable;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _interactable = GetComponent<Interactable>();
    }

    void Update()
    {
        if (string.IsNullOrEmpty(boolParameterName)) return;

        // Continuously sync the animator parameter with the interactable's state
        _animator.SetBool(boolParameterName, _interactable.IsActivated);
    }
}
