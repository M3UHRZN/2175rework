using System.Collections;
using UnityEngine;

[AddComponentMenu("Interactions/Actions/Screen Fade Action")]
public class ScreenFadeAction : InteractActionBase
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] float startAlpha = 0f;
    [SerializeField] float completeAlpha = 1f;
    [SerializeField] float fadeDuration = 0.4f;

    Coroutine fadeRoutine;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!canvasGroup)
            canvasGroup = GetComponentInChildren<CanvasGroup>();
    }

    protected override void OnStart(InteractionController controller)
    {
        BeginFade(startAlpha);
    }

    protected override void OnComplete(InteractionController controller)
    {
        BeginFade(completeAlpha);
    }

    void BeginFade(float targetAlpha)
    {
        if (!canvasGroup)
            return;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    IEnumerator FadeRoutine(float target)
    {
        if (!canvasGroup)
            yield break;
        float initial = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, fadeDuration));
            float alpha = Mathf.Lerp(initial, target, t);
            InteractUtils.SetCanvasGroupAlpha(canvasGroup, alpha);
            yield return null;
        }
        InteractUtils.SetCanvasGroupAlpha(canvasGroup, target);
        fadeRoutine = null;
    }
}
