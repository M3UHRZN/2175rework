using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Dialogue Action")]
public class DialogueAction : InteractActionBase
{
    [SerializeField] string dialogueId;
    [SerializeField] string nodeId;
    [SerializeField] UnityEvent onDialogueStart;
    [SerializeField] UnityEvent onDialogueAdvance;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, false);
    }

    protected override void OnStart(InteractionController controller)
    {
        onDialogueStart?.Invoke();
    }

    protected override void OnComplete(InteractionController controller)
    {
        onDialogueAdvance?.Invoke();
    }
}
