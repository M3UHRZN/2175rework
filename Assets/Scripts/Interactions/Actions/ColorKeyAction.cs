using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Color Key Action")]
public class ColorKeyAction : InteractActionBase
{
    [Header("Key Settings")]
    [SerializeField] string requiredKeyId;
    [SerializeField] Transform unlockedVisual;
    [SerializeField] Transform lockedVisual;
    [SerializeField] AudioSource successAudio;
    [SerializeField] AudioSource failureAudio;

    bool unlocked;

    protected override void Awake()
    {
        base.Awake();
        ApplyState();
    }

    protected override void OnComplete(InteractionController controller)
    {
        unlocked = CheckKey(controller);
        ApplyState();
    }

    bool CheckKey(InteractionController controller)
    {
        if (!controller)
            return false;
        var keyRing = controller.GetComponent<ColorKeyRing>();
        if (!keyRing)
            return false;
        bool success = string.IsNullOrEmpty(requiredKeyId) || keyRing.CurrentKeyId == requiredKeyId;
        if (success)
        {
            if (successAudio)
                successAudio.Play();
        }
        else if (failureAudio)
        {
            failureAudio.Play();
        }
        return success;
    }

    void ApplyState()
    {
        InteractUtils.SetActiveSafe(unlockedVisual, unlocked);
        InteractUtils.SetActiveSafe(lockedVisual, !unlocked);
    }
}
