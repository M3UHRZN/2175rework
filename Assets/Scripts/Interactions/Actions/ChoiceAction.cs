using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Choice Action")]
public class ChoiceAction : InteractActionBase
{
    [System.Serializable]
    public class ChoiceSelectedEvent : UnityEvent<int> { }

    [Header("Choice UI")]
    [SerializeField] CanvasGroup choiceCanvas;
    [SerializeField] string gateFlagName = "Choice/Selection";
    [SerializeField] ChoiceSelectedEvent onChoiceSelected = new ChoiceSelectedEvent();

    bool open;

    protected override void Awake()
    {
        base.Awake();
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        open = true;
        ApplyState();
    }

    public void HandleChoice(int index)
    {
        if (!open)
            return;

        InteractionStateStore.SetFlag(gateFlagName, index == 0);
        onChoiceSelected.Invoke(index);
        open = false;
        ApplyState();
    }

    void ApplyState()
    {
        if (choiceCanvas)
        {
            InteractUtils.SetCanvasGroupVisible(choiceCanvas, open);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        open = false;
        ApplyState();
    }
}
