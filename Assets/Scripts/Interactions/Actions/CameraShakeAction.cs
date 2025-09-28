using UnityEngine;
using Cinemachine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Camera Shake Action")]
public class CameraShakeAction : InteractActionBase
{
    [Header("Camera Shake")]
    [SerializeField] CinemachineImpulseSource impulseSource;

    protected override void OnComplete(InteractionController controller)
    {
        if (impulseSource)
        {
            impulseSource.GenerateImpulse();
        }
    }
}
