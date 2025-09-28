using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Light Toggle Action")]
public class LightToggleAction : InteractActionBase
{
    [Header("Light Toggle")]
    [SerializeField] Light targetLight;
    [SerializeField] float onIntensity = 1f;
    [SerializeField] float offIntensity = 0f;
    [SerializeField] bool startOn = true;

    bool isOn;

    protected override void Awake()
    {
        base.Awake();
        isOn = startOn;
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        isOn = !isOn;
        ApplyState();
    }

    void ApplyState()
    {
        if (!targetLight)
            return;

        InteractUtils.SetLightEnabled(targetLight, isOn);
        targetLight.intensity = isOn ? onIntensity : offIntensity;
    }
}
