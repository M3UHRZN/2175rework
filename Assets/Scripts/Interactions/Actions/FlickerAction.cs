using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Flicker Action")]
public class FlickerAction : InteractActionBase
{
    [Header("Flicker")]
    [SerializeField] Light targetLight;
    [SerializeField] Renderer[] renderers;
    [SerializeField] float intervalMs = 120f;

    bool flickering;
    bool state;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
        ApplyState(false);
    }

    protected override void OnStart(InteractionController controller)
    {
        BeginFlicker();
    }

    protected override void OnCancel(InteractionController controller)
    {
        EndFlicker();
    }

    protected override void OnComplete(InteractionController controller)
    {
        EndFlicker();
    }

    void BeginFlicker()
    {
        if (flickering)
            return;
        flickering = true;
        state = true;
        ApplyState(state);
        float interval = Mathf.Max(0.01f, intervalMs * 0.001f);
        InvokeRepeating(nameof(Toggle), interval, interval);
    }

    void EndFlicker()
    {
        if (!flickering)
            return;
        flickering = false;
        CancelInvoke(nameof(Toggle));
        state = true;
        ApplyState(state);
    }

    void Toggle()
    {
        state = !state;
        ApplyState(state);
    }

    void ApplyState(bool enabled)
    {
        if (targetLight)
        {
            InteractUtils.SetLightEnabled(targetLight, enabled);
        }
        if (renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                InteractUtils.SetRendererEnabled(renderers[i], enabled);
            }
        }
    }

    protected override void OnDisable()
    {
        EndFlicker();
        base.OnDisable();
    }
}
