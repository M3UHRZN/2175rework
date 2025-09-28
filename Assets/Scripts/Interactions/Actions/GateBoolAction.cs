using UnityEngine;

[AddComponentMenu("Interactions/Actions/Gate Bool Action")]
public class GateBoolAction : InteractActionBase
{
    [SerializeField] string flagId;
    [SerializeField] bool setOnStart;
    [SerializeField] bool setOnComplete = true;
    [SerializeField] bool clearOnCancel;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        if (setOnStart)
            InteractionStateRegistry.SetBool(flagId, true);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (setOnComplete)
            InteractionStateRegistry.SetBool(flagId, true);
    }

    protected override void OnCancel(InteractionController controller)
    {
        if (clearOnCancel)
            InteractionStateRegistry.SetBool(flagId, false);
    }

    public void Clear()
    {
        InteractionStateRegistry.SetBool(flagId, false);
    }
}
