using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Bindings")]
    [Tooltip("Konuşma karakter kimliklerini PlayerOrchestrator referansları ile eşler.")]
    public List<DialogueCharacterBinding> characterBindings = new List<DialogueCharacterBinding>();

    [Header("UI")]
    [Tooltip("Konuşma satırlarını gösterecek UI bileşeni.")]
    public DialogueUI dialogueUI;

    [Header("Input")]
    [Tooltip("Konuşmayı ilerletmek için kullanılacak InputAction (ör: Space tuşu).")]
    public InputActionReference advanceAction;

    [Tooltip("Yeni bir diyalog başladığında tetiklenecek olay.")]
    public UnityEvent<DialogueSequence> onDialogueStarted;

    [Tooltip("Bir diyalog satırı gösterildiğinde tetiklenecek olay.")]
    public UnityEvent<DialogueSequence.DialogueLine> onLineChanged;

    [Tooltip("Diyalog tamamen bittiğinde tetiklenecek olay.")]
    public UnityEvent<DialogueSequence> onDialogueFinished;

    DialogueSequence currentSequence;
    int currentIndex = -1;
    DialogueCharacterBinding lockedBinding;
    bool allLocked;
    Coroutine autoAdvanceRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple DialogueManager instances found. Destroying duplicate.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        if (advanceAction && advanceAction.action != null)
        {
            advanceAction.action.performed += OnAdvanceAction;
            advanceAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (advanceAction && advanceAction.action != null)
        {
            advanceAction.action.performed -= OnAdvanceAction;
            advanceAction.action.Disable();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void OnAdvanceAction(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Advance();
    }

    public bool StartDialogue(DialogueSequence sequence)
    {
        if (sequence == null)
            return false;

        if (currentSequence != null)
        {
            Debug.LogWarning("DialogueManager is already playing a sequence. Skipping new request.", this);
            return false;
        }

        currentSequence = sequence;
        currentIndex = -1;
        onDialogueStarted?.Invoke(sequence);
        ShowNextLine();
        return true;
    }

    public bool IsDialogueRunning() => currentSequence != null;

    public void Advance()
    {
        if (currentSequence == null)
            return;

        ShowNextLine();
    }

    void ShowNextLine()
    {
        UnlockMovement();
        CancelAutoAdvance();

        currentIndex++;
        if (currentSequence == null || currentSequence.lines == null || currentIndex >= currentSequence.lines.Count)
        {
            FinishDialogue();
            return;
        }

        var line = currentSequence.lines[currentIndex];
        if (line == null)
        {
            ShowNextLine();
            return;
        }

        LockMovement(line);
        dialogueUI?.ShowLine(line.speakerDisplayName, line.speakerId, line.text);
        onLineChanged?.Invoke(line);
        ScheduleAutoAdvance(line, currentIndex);
    }

    void LockMovement(DialogueSequence.DialogueLine line)
    {
        lockedBinding = null;
        allLocked = false;

        if (line.movementLock == DialogueMovementLock.None)
            return;

        if (line.movementLock == DialogueMovementLock.AllBoundCharacters)
        {
            foreach (var binding in characterBindings)
            {
                binding?.OverrideInputEnabled(false);
            }
            allLocked = true;
            return;
        }

        if (line.movementLock == DialogueMovementLock.Speaker)
        {
            lockedBinding = GetBinding(line.speakerId);
            lockedBinding?.OverrideInputEnabled(false);
        }
    }

    void UnlockMovement()
    {
        if (allLocked)
        {
            foreach (var binding in characterBindings)
            {
                binding?.RestoreInputState();
            }
            allLocked = false;
        }

        if (lockedBinding != null)
        {
            lockedBinding.RestoreInputState();
            lockedBinding = null;
        }
    }

    void FinishDialogue()
    {
        var finishedSequence = currentSequence;
        UnlockMovement();
        CancelAutoAdvance();
        dialogueUI?.Hide();
        currentSequence = null;
        currentIndex = -1;
        if (finishedSequence != null)
        {
            finishedSequence.onSequenceFinished?.Invoke();
            onDialogueFinished?.Invoke(finishedSequence);
        }
    }

    void ScheduleAutoAdvance(DialogueSequence.DialogueLine line, int lineIndex)
    {
        if (line == null || line.autoAdvanceDelay <= 0f)
            return;

        autoAdvanceRoutine = StartCoroutine(AutoAdvanceAfterDelay(line.autoAdvanceDelay, lineIndex));
    }

    IEnumerator AutoAdvanceAfterDelay(float delay, int lineIndex)
    {
        yield return new WaitForSeconds(delay);

        if (currentSequence == null || currentIndex != lineIndex)
            yield break;

        autoAdvanceRoutine = null;
        Advance();
    }

    void CancelAutoAdvance()
    {
        if (autoAdvanceRoutine != null)
        {
            StopCoroutine(autoAdvanceRoutine);
            autoAdvanceRoutine = null;
        }
    }

    public DialogueCharacterBinding GetBinding(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
            return null;

        for (int i = 0; i < characterBindings.Count; i++)
        {
            var binding = characterBindings[i];
            if (binding != null && binding.HasId(characterId))
                return binding;
        }

        return null;
    }

    public DialogueCharacterBinding GetBinding(PlayerOrchestrator orchestrator)
    {
        if (!orchestrator)
            return null;

        for (int i = 0; i < characterBindings.Count; i++)
        {
            var binding = characterBindings[i];
            if (binding != null && binding.controller == orchestrator)
                return binding;
        }

        return null;
    }

    public bool MatchesCharacter(PlayerOrchestrator orchestrator, string requiredId)
    {
        if (string.IsNullOrEmpty(requiredId))
            return true;

        for (int i = 0; i < characterBindings.Count; i++)
        {
            var binding = characterBindings[i];
            if (binding != null && binding.controller == orchestrator && binding.HasId(requiredId))
                return true;
        }

        return false;
    }
}

[System.Serializable]
public class DialogueCharacterBinding
{
    [Tooltip("Diyaloglarda kullanılacak benzersiz karakter kimliği (örn. 'Elior').")]
    public string characterId;

    [Tooltip("Bu kimlikle ilişkili PlayerOrchestrator bileşeni.")]
    public PlayerOrchestrator controller;

    InputAdapter cachedInput;
    bool hasOverride;
    bool cachedState;

    public bool HasId(string id)
    {
        if (string.IsNullOrEmpty(id))
            return false;

        return string.Equals(characterId, id, System.StringComparison.OrdinalIgnoreCase);
    }

    public void OverrideInputEnabled(bool enabled)
    {
        if (!controller)
            return;

        if (!cachedInput)
            cachedInput = controller.GetComponent<InputAdapter>();

        if (!cachedInput)
            return;

        if (!hasOverride)
        {
            cachedState = cachedInput.InputEnabled;
            hasOverride = true;
        }

        cachedInput.InputEnabled = enabled;
    }

    public void RestoreInputState()
    {
        if (!hasOverride)
            return;

        if (!controller)
        {
            hasOverride = false;
            return;
        }

        if (!cachedInput)
            cachedInput = controller.GetComponent<InputAdapter>();

        if (cachedInput)
            cachedInput.InputEnabled = cachedState;

        hasOverride = false;
    }
}
