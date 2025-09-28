using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Gate Bool Action")]
public class GateBoolAction : InteractActionBase
{
    [Header("Gate Bool")]
    [SerializeField] string flagName = "Gate/Flag";
    [SerializeField] bool valueOnComplete = true;
    [SerializeField] bool resetOnCancel = true;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        if (resetOnCancel)
        {
            InteractionStateStore.SetFlag(flagName, false);
        }
    }

    protected override void OnComplete(InteractionController controller)
    {
        InteractionStateStore.SetFlag(flagName, valueOnComplete);
    }

    protected override void OnCancel(InteractionController controller)
    {
        if (resetOnCancel)
        {
            InteractionStateStore.SetFlag(flagName, false);
        }
    }

    public bool GetFlag()
    {
        return InteractionStateStore.GetFlag(flagName);
    }
}
