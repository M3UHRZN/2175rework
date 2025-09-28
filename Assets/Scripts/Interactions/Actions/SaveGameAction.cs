using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Save Game Action")]
public class SaveGameAction : InteractActionBase
{
    [SerializeField] UnityEvent onSave;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onSave?.Invoke();
    }
}
