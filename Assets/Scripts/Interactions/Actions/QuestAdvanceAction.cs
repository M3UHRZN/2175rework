using UnityEngine;
using UnityEngine.Events;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Quest Advance Action")]
public class QuestAdvanceAction : InteractActionBase
{
    [System.Serializable]
    public class QuestEvent : UnityEvent<string> { }

    [Header("Quest")]
    [SerializeField] string questId = "MainQuest";
    [SerializeField] int step = 1;
    [SerializeField] QuestEvent onQuestAdvance = new QuestEvent();

    protected override void OnComplete(InteractionController controller)
    {
        onQuestAdvance.Invoke(questId + ":" + step);
    }
}
