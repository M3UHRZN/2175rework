using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Platform Toggle Action")]
public class PlatformToggleAction : InteractActionBase
{
    [Header("Platform Toggle")]
    [SerializeField] Transform targetRoot;
    [SerializeField] bool startEnabled = true;

    bool isEnabled;

    protected override void Awake()
    {
        base.Awake();
        isEnabled = startEnabled;
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        isEnabled = !isEnabled;
        ApplyState();
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(targetRoot, isEnabled);
    }
}
