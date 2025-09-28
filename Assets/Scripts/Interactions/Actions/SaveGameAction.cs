using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Save Game Action")]
public class SaveGameAction : InteractActionBase
{
    [Header("Save Game")]
    [SerializeField] UnityEvent onSaveRequested = new UnityEvent();

    protected override void OnComplete(InteractionController controller)
    {
        onSaveRequested.Invoke();
    }
}
