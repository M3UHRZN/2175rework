using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ChoiceEvent : UnityEvent<int> { }

[AddComponentMenu("Interactions/Actions/Choice Action")]
public class ChoiceAction : InteractActionBase
{
    [SerializeField] string sharedFlagId = "choice";
    [SerializeField] ChoiceEvent onShowChoices;
    [SerializeField] ChoiceEvent onChoiceSelected;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onShowChoices?.Invoke(0);
    }

    public void ResolveChoice(int index)
    {
        InteractionStateRegistry.SetInt(sharedFlagId, index);
        onChoiceSelected?.Invoke(index);
    }
}
