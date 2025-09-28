using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Dialogue Action")]
public class DialogueAction : InteractActionBase
{
    [System.Serializable]
    public class DialogueEvent : UnityEvent<string> { }

    [Header("Dialogue")]
    [SerializeField] string dialogueId = "Dialogue/Intro";
    [SerializeField] DialogueEvent onDialogueStart = new DialogueEvent();
    [SerializeField] DialogueEvent onDialogueAdvance = new DialogueEvent();

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        onDialogueStart.Invoke(dialogueId);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onDialogueAdvance.Invoke(dialogueId);
    }

    protected override void OnCancel(InteractionController controller)
    {
        onDialogueAdvance.Invoke(string.Empty);
    }
}
