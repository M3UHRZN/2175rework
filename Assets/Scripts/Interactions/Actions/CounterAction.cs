using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Counter Action")]
public class CounterAction : InteractActionBase
{
    [Header("Counter")]
    [SerializeField] string counterName = "Counter/Generic";
    [SerializeField] int targetCount = 3;
    [SerializeField] UnityEvent onThresholdReached = new UnityEvent();

    protected override void OnComplete(InteractionController controller)
    {
        int value = InteractionStateStore.IncrementCounter(counterName);
        if (value >= targetCount)
        {
            onThresholdReached.Invoke();
        }
    }

    public void ResetCounter()
    {
        InteractionStateStore.ResetCounter(counterName);
    }
}
