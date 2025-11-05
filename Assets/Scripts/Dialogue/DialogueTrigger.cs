using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [Tooltip("Tetiklendiğinde oynatılacak diyalog dizisi.")]
    public DialogueSequence dialogue;

    [Tooltip("Bu tetikleyiciyi çalıştırabilecek karakter kimliği. Boş bırakılırsa herkes tetikleyebilir.")]
    public string requiredCharacterId;

    [Tooltip("Diyalog başladıktan sonra tetikleyici tekrar kullanılmasın istiyorsanız işaretleyin.")]
    public bool triggerOnce = true;

    [Tooltip("Diyalog sırasında tetikleyici karakterin hareketini kilitle.")]
    public bool lockWhileRunning = false;

    bool hasTriggered;
    DialogueCharacterBinding activeBinding;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col)
            col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled || dialogue == null)
            return;

        if (triggerOnce && hasTriggered)
            return;

        var orchestrator = other.GetComponent<PlayerOrchestrator>();
        if (!orchestrator)
            orchestrator = other.GetComponentInParent<PlayerOrchestrator>();

        if (!orchestrator)
            return;

        if (!DialogueManager.Instance)
        {
            Debug.LogWarning("DialogueTrigger: DialogueManager bulunamadı.", this);
            return;
        }

        if (!DialogueManager.Instance.MatchesCharacter(orchestrator, requiredCharacterId))
            return;

        if (lockWhileRunning)
        {
            DialogueManager.Instance.onDialogueFinished.AddListener(OnDialogueFinished);
            activeBinding = !string.IsNullOrEmpty(requiredCharacterId)
                ? DialogueManager.Instance.GetBinding(requiredCharacterId)
                : DialogueManager.Instance.GetBinding(orchestrator);

            activeBinding?.OverrideInputEnabled(false);
        }

        bool started = DialogueManager.Instance.StartDialogue(dialogue);
        if (!started && lockWhileRunning)
        {
            DialogueManager.Instance.onDialogueFinished.RemoveListener(OnDialogueFinished);
            activeBinding?.RestoreInputState();
            activeBinding = null;
        }

        if (started)
            hasTriggered = true;
    }

    void OnDialogueFinished(DialogueSequence finished)
    {
        if (!lockWhileRunning)
            return;

        if (!DialogueManager.Instance)
            return;

        DialogueManager.Instance.onDialogueFinished.RemoveListener(OnDialogueFinished);

        if (finished != dialogue)
            return;

        activeBinding?.RestoreInputState();
        activeBinding = null;
    }
}
