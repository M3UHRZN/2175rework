using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Multi Gate Action")]
public class MultiGateAction : InteractActionBase
{
    [Header("Multi Gate")]
    [SerializeField] string[] requiredFlags;
    [SerializeField] UnityEvent onAllTrue = new UnityEvent();

    protected override void OnComplete(InteractionController controller)
    {
        if (AllFlagsTrue())
        {
            onAllTrue.Invoke();
        }
    }

    bool AllFlagsTrue()
    {
        if (requiredFlags == null || requiredFlags.Length == 0)
            return true;
        for (int i = 0; i < requiredFlags.Length; i++)
        {
            if (!InteractionStateStore.GetFlag(requiredFlags[i]))
                return false;
        }
        return true;
    }
}
