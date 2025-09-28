using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Multi Gate Action")]
public class MultiGateAction : InteractActionBase
{
    [SerializeField] string[] requiredFlags;
    [SerializeField] UnityEvent onAllTrue;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
        {
            onAllTrue?.Invoke();
            return;
        }

        for (int i = 0; i < requiredFlags.Length; i++)
        {
            if (!InteractionStateRegistry.GetBool(requiredFlags[i]))
                return;
        }

        onAllTrue?.Invoke();
    }
}
