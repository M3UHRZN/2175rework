using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SequenceStepEvent : UnityEvent<int> { }

[AddComponentMenu("Interactions/Actions/Sequence Pad Action")]
public class SequencePadAction : InteractActionBase
{
    [SerializeField] int padIndex;
    [SerializeField] SequenceStepEvent onStep;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onStep?.Invoke(padIndex);
    }
}
