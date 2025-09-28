using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Reticle Snap Action")]
public class ReticleSnapAction : InteractActionBase
{
    [SerializeField] RectTransform reticle;
    [SerializeField] Transform worldTarget;
    [SerializeField] Vector3 screenOffset;
    [SerializeField] UnityEvent onSnap;
    [SerializeField] UnityEvent onRelease;

    Vector3 originalAnchored;
    bool snapped;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, true, true, false, false, true);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (reticle)
            originalAnchored = reticle.anchoredPosition3D;
    }

    protected override void OnFocusEnter(InteractionController controller)
    {
        CacheOriginal();
    }

    protected override void OnStart(InteractionController controller)
    {
        if (!reticle || snapped)
            return;
        CacheOriginal();
        if (worldTarget)
        {
            Vector3 screenPos = Camera.main ? Camera.main.WorldToScreenPoint(worldTarget.position) : Vector3.zero;
            reticle.position = screenPos + screenOffset;
        }
        snapped = true;
        onSnap?.Invoke();
    }

    protected override void OnCancel(InteractionController controller)
    {
        Release();
    }

    protected override void OnFocusExit(InteractionController controller)
    {
        Release();
    }

    void CacheOriginal()
    {
        if (reticle)
            originalAnchored = reticle.anchoredPosition3D;
    }

    void Release()
    {
        if (!reticle || !snapped)
            return;
        reticle.anchoredPosition3D = originalAnchored;
        snapped = false;
        onRelease?.Invoke();
    }
}
