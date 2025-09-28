using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Timed Switch Action")]
public class TimedSwitchAction : InteractActionBase
{
    [Header("Timed Switch")]
    [SerializeField] Transform activeRoot;
    [SerializeField] Transform inactiveRoot;
    [SerializeField] float durationMs = 2000f;

    bool isActive;

    protected override void Awake()
    {
        base.Awake();
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        isActive = true;
        ApplyState();
        CancelInvoke(nameof(TurnOff));
        if (durationMs > 0f)
        {
            Invoke(nameof(TurnOff), durationMs * 0.001f);
        }
    }

    void TurnOff()
    {
        isActive = false;
        ApplyState();
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(activeRoot, isActive);
        InteractUtils.SetActiveSafe(inactiveRoot, !isActive);
    }

    protected override void OnDisable()
    {
        CancelInvoke(nameof(TurnOff));
        base.OnDisable();
    }
}
