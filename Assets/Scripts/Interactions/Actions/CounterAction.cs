using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Counter Action")]
public class CounterAction : InteractActionBase
{
    [SerializeField] string counterId;
    [SerializeField] int requiredCount = 3;
    [SerializeField] UnityEvent onThresholdReached;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (string.IsNullOrEmpty(counterId))
            counterId = name;
        int current = InteractionStateRegistry.GetInt(counterId) + 1;
        InteractionStateRegistry.SetInt(counterId, current);
        if (current >= requiredCount)
            onThresholdReached?.Invoke();
    }

    public void ResetCounter()
    {
        if (string.IsNullOrEmpty(counterId))
            counterId = name;
        InteractionStateRegistry.SetInt(counterId, 0);
    }
}
