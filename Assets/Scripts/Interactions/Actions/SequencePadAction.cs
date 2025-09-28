using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Sequence Pad Action")]
public class SequencePadAction : InteractActionBase
{
    [System.Serializable]
    public class SequenceEvent : UnityEvent<int> { }

    [Header("Sequence Pad")]
    [SerializeField] int stepId = 1;
    [SerializeField] SequenceEvent onSequenceStep = new SequenceEvent();

    protected override void OnComplete(InteractionController controller)
    {
        onSequenceStep.Invoke(stepId);
    }
}
