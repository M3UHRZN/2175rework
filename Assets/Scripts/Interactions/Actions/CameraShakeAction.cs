using System.Collections;
using UnityEngine;

[AddComponentMenu("Interactions/Actions/Camera Shake Action")]
public class CameraShakeAction : InteractActionBase
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float amplitude = 0.2f;
    [SerializeField] float duration = 0.3f;
    [SerializeField] float frequency = 35f;

    Vector3 originalPosition;
    Coroutine shakeRoutine;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (!cameraTransform && Camera.main)
            cameraTransform = Camera.main.transform;
        if (cameraTransform)
            originalPosition = cameraTransform.localPosition;
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (!cameraTransform)
            return;
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    IEnumerator ShakeRoutine()
    {
        if (!cameraTransform)
            yield break;
        originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            float offsetX = (Mathf.PerlinNoise(Time.time * frequency, 0f) - 0.5f) * 2f * amplitude * damper;
            float offsetY = (Mathf.PerlinNoise(0f, Time.time * frequency) - 0.5f) * 2f * amplitude * damper;
            cameraTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        cameraTransform.localPosition = originalPosition;
        shakeRoutine = null;
    }
}
