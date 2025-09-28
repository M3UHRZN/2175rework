using UnityEngine;

[AddComponentMenu("Interactions/Actions/Platform Toggle Action")]
public class PlatformToggleAction : InteractActionBase
{
    [SerializeField] GameObject targetPlatform;
    bool isActive = true;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        InteractUtils.SetActiveSafe(targetPlatform, isActive);
    }

    protected override void OnComplete(InteractionController controller)
    {
        isActive = !isActive;
        InteractUtils.SetActiveSafe(targetPlatform, isActive);
    }
}
