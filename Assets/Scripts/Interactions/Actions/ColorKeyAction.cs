using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Color Key Action")]
public class ColorKeyAction : InteractActionBase
{
    [SerializeField] string requiredKeyId;
    [SerializeField] string gateFlagId;
    [SerializeField] SpriteRenderer feedbackRenderer;
    [SerializeField] Color successColor = Color.green;
    [SerializeField] Color failureColor = Color.red;
    [SerializeField] UnityEvent onSuccess;
    [SerializeField] UnityEvent onFailure;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        bool hasKey = InteractionStateRegistry.GetBool(requiredKeyId);
        if (hasKey)
        {
            if (!string.IsNullOrEmpty(gateFlagId))
                InteractionStateRegistry.SetBool(gateFlagId, true);
            if (feedbackRenderer)
                feedbackRenderer.color = successColor;
            onSuccess?.Invoke();
        }
        else
        {
            if (feedbackRenderer)
                feedbackRenderer.color = failureColor;
            onFailure?.Invoke();
        }
    }
}
