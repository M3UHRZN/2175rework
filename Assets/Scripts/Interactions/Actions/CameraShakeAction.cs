using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Camera Shake Action")]
public class CameraShakeAction : InteractActionBase
{
    [Header("Camera Shake")]
    [SerializeField] Component impulseSource; // Generic Component to avoid Cinemachine dependency
    
    [Header("Alternative Shake")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] float shakeIntensity = 0.5f;
    [SerializeField] float shakeDuration = 0.2f;

    protected override void OnComplete(InteractionController controller)
    {
        // Try Cinemachine first if available
        if (impulseSource != null)
        {
            var impulseMethod = impulseSource.GetType().GetMethod("GenerateImpulse");
            if (impulseMethod != null)
            {
                impulseMethod.Invoke(impulseSource, null);
                return;
            }
        }
        
        // Fallback to simple camera shake
        if (cameraTransform != null)
        {
            StartCoroutine(SimpleCameraShake());
        }
    }
    
    System.Collections.IEnumerator SimpleCameraShake()
    {
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            
            cameraTransform.localPosition = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        cameraTransform.localPosition = originalPosition;
    }
}
