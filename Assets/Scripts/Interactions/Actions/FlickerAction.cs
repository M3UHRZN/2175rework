using UnityEngine;

[AddComponentMenu("Interactions/Actions/Flicker Action")]
public class FlickerAction : InteractActionBase
{
    [SerializeField] Light targetLight;
    [SerializeField] float interval = 0.1f;
    [SerializeField] float variance = 0.05f;
    [SerializeField] float minIntensity = 0.2f;
    [SerializeField] float maxIntensity = 1f;

    float baseIntensity;
    bool flickering;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (targetLight)
            baseIntensity = targetLight.intensity;
    }

    protected override void OnStart(InteractionController controller)
    {
        if (!targetLight || flickering)
            return;
        baseIntensity = targetLight.intensity;
        flickering = true;
        InvokeRepeating(nameof(DoFlicker), 0f, Mathf.Max(0.01f, interval));
    }

    protected override void OnComplete(InteractionController controller)
    {
        StopFlicker();
    }

    protected override void OnCancel(InteractionController controller)
    {
        StopFlicker();
    }

    void DoFlicker()
    {
        if (!targetLight)
            return;
        float t = Random.Range(minIntensity, maxIntensity);
        targetLight.intensity = t;
    }

    void StopFlicker()
    {
        if (!flickering)
            return;
        flickering = false;
        CancelInvoke(nameof(DoFlicker));
        if (targetLight)
            targetLight.intensity = baseIntensity;
    }
}
