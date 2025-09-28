using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("Interactions/Actions/Quest Advance Action")]
public class QuestAdvanceAction : InteractActionBase
{
    [SerializeField] string questId;
    [SerializeField] UnityEvent onAdvance;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (!string.IsNullOrEmpty(questId))
        {
            int current = InteractionStateRegistry.GetInt(questId);
            InteractionStateRegistry.SetInt(questId, current + 1);
        }
        onAdvance?.Invoke();
    }
}
