using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Memory Flash Action")]
public class MemoryFlashAction : InteractActionBase
{
    [SerializeField] UnityEvent onFlash;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        onFlash?.Invoke();
    }
}
