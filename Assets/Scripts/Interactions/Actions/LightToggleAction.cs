using UnityEngine;

[AddComponentMenu("Interactions/Actions/Light Toggle Action")]
public class LightToggleAction : InteractActionBase
{
    [SerializeField] Light targetLight;
    [SerializeField] float offIntensity = 0f;
    [SerializeField] float onIntensity = 1f;
    [SerializeField] bool startEnabled = true;

    bool isOn;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        isOn = startEnabled;
        Apply();
    }

    protected override void OnComplete(InteractionController controller)
    {
        isOn = !isOn;
        Apply();
    }

    void Apply()
    {
        if (targetLight)
        {
            targetLight.enabled = isOn;
            targetLight.intensity = isOn ? onIntensity : offIntensity;
        }
    }
}
