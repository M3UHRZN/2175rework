using UnityEngine;
using UnityEngine.UI;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Memory Flash Action")]
public class MemoryFlashAction : InteractActionBase
{
    [Header("Memory Flash")]
    [SerializeField] Animator flashAnimator;
    [SerializeField] string triggerName = "Flash";
    [SerializeField] Text messageText;
    [SerializeField] string message;

    protected override void OnComplete(InteractionController controller)
    {
        if (flashAnimator && !string.IsNullOrEmpty(triggerName))
        {
            flashAnimator.SetTrigger(triggerName);
        }
        if (messageText)
        {
            messageText.text = message;
        }
    }
}
