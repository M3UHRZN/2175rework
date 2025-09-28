using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Elevator Call Action")]
public class ElevatorCallAction : InteractActionBase
{
    [Header("Elevator Call")]
    [SerializeField] Animator targetAnimator;
    [SerializeField] string triggerName = "Call";
    [Tooltip("Optional audio clip for the elevator call.")]
    [SerializeField] AudioSource callAudio;

    protected override void OnComplete(InteractionController controller)
    {
        if (targetAnimator && !string.IsNullOrEmpty(triggerName))
        {
            targetAnimator.SetTrigger(triggerName);
        }
        if (callAudio)
        {
            callAudio.Play();
        }
    }
}
