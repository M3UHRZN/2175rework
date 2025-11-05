using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Game.Settings;

/// <summary>
/// Handles cooperative level transitions that require both main characters to stand
/// inside the same trigger volume before advancing to the next scene.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class CoopLevelExit : MonoBehaviour
{
    private const int OverlapBufferSize = 16;

    [Header("Characters")]
    [Tooltip("Party controller that owns both protagonists. Used to query merge/split state.")]
    [SerializeField] DualCharacterController partyController;

    [Tooltip("Reference to Elior's orchestrator component.")]
    [SerializeField] PlayerOrchestrator elior;

    [Tooltip("Reference to Sim's orchestrator component.")]
    [SerializeField] PlayerOrchestrator sim;

    [Tooltip("When enabled, the exit can only be activated while the party is split (both avatars visible).")]
    [SerializeField] bool requireSplitState = true;

    [Header("Transition")]
    [Tooltip("Explicit name of the scene that should be loaded next. Leave empty to use the Build Index Offset instead.")]
    [SerializeField] string nextSceneName;

    [Tooltip("Relative build index to load when Next Scene Name is empty.")]
    [SerializeField] int buildIndexOffset = 1;

    [Tooltip("Optional delay (in seconds) before the scene transition happens once the requirements are met.")]
    [SerializeField] float loadDelay = 0.5f;

    [Tooltip("Automatically begin loading the next scene as soon as the requirements are satisfied.")]
    [SerializeField] bool autoLoadScene = true;

    [Header("Puzzle State Integration")]
    [Tooltip("Optional flag name pushed to the PuzzleStateManager while both characters stand inside the exit.")]
    [SerializeField] string readyFlagName;

    [Tooltip("Clear the puzzle flag automatically when one of the characters leaves the exit volume.")]
    [SerializeField] bool clearFlagWhenCharactersLeave = true;

    [Header("Events")]
    [Tooltip("Invoked the first frame both characters satisfy the requirements.")]
    public UnityEvent OnBothCharactersInside;

    [Tooltip("Invoked when the requirements are no longer satisfied.")]
    public UnityEvent OnRequirementsLost;

    [Tooltip("Invoked right before the scene loading routine runs (after the optional delay).")]
    public UnityEvent OnSceneLoadStarted;

    readonly HashSet<Collider2D> eliorContacts = new HashSet<Collider2D>();
    readonly HashSet<Collider2D> simContacts = new HashSet<Collider2D>();
    readonly Collider2D[] overlapBuffer = new Collider2D[OverlapBufferSize];

    Collider2D triggerCollider;
    PuzzleStateManager puzzleManager;
    Coroutine pendingLoadRoutine;
    bool requirementsSatisfied;
    bool sceneLoadQueued;
    bool flagRaised;
    bool warnedMissingPartyController;

    void Reset()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider)
        {
            triggerCollider.isTrigger = true;
        }

        AutoAssignCharacters();
    }

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[CoopLevelExit] Collider on {name} was not set as a trigger. Enabling trigger mode automatically.", this);
            triggerCollider.isTrigger = true;
        }

        AutoAssignCharacters();
        puzzleManager = FindFirstObjectByType<PuzzleStateManager>();
        UpdatePuzzleFlag(false);
    }

    void OnEnable()
    {
        SubscribeToPartyEvents();
        RefreshCurrentOverlaps();
    }

    void OnDisable()
    {
        UnsubscribeFromPartyEvents();

        if (pendingLoadRoutine != null)
        {
            StopCoroutine(pendingLoadRoutine);
            pendingLoadRoutine = null;
        }

        sceneLoadQueued = false;
        requirementsSatisfied = false;
        eliorContacts.Clear();
        simContacts.Clear();

        if (flagRaised && clearFlagWhenCharactersLeave)
        {
            UpdatePuzzleFlag(false);
        }
    }

    void AutoAssignCharacters()
    {
        if (!partyController)
        {
            partyController = FindFirstObjectByType<DualCharacterController>();
            if (!partyController && !warnedMissingPartyController && requireSplitState && Application.isPlaying)
            {
                Debug.LogWarning("[CoopLevelExit] DualCharacterController not found. Assign the Party Controller reference or disable Require Split State.", this);
                warnedMissingPartyController = true;
            }
        }

        if (partyController)
        {
            if (!elior)
            {
                elior = partyController.elior;
            }
            if (!sim)
            {
                sim = partyController.sim;
            }
        }

        if ((!elior || !sim) && Application.isPlaying)
        {
            Debug.LogWarning("[CoopLevelExit] Character references are missing. Assign both protagonists in the inspector.", this);
        }
    }

    void SubscribeToPartyEvents()
    {
        if (partyController != null)
        {
            partyController.OnMergedStateChanged.AddListener(OnMergedStateChanged);
        }
    }

    void UnsubscribeFromPartyEvents()
    {
        if (partyController != null)
        {
            partyController.OnMergedStateChanged.RemoveListener(OnMergedStateChanged);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var actor = ResolveActor(other);
        if (!actor)
        {
            return;
        }

        if (actor == elior)
        {
            if (eliorContacts.Add(other))
            {
                EvaluateRequirements();
            }
        }
        else if (actor == sim)
        {
            if (simContacts.Add(other))
            {
                EvaluateRequirements();
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var actor = ResolveActor(other);
        if (!actor)
        {
            return;
        }

        if (actor == elior)
        {
            if (eliorContacts.Remove(other))
            {
                EvaluateRequirements();
            }
        }
        else if (actor == sim)
        {
            if (simContacts.Remove(other))
            {
                EvaluateRequirements();
            }
        }
    }

    void OnMergedStateChanged(bool merged)
    {
        if (merged)
        {
            simContacts.Clear();
        }

        RefreshCurrentOverlaps();
    }

    void RefreshCurrentOverlaps()
    {
        eliorContacts.Clear();
        simContacts.Clear();

        if (!triggerCollider)
        {
            return;
        }

        var filter = new ContactFilter2D
        {
            useTriggers = true,
            useLayerMask = false,
            useDepth = false
        };

        int count = Physics2D.OverlapCollider(triggerCollider, filter, overlapBuffer);
        for (int i = 0; i < count; i++)
        {
            var candidate = overlapBuffer[i];
            if (!candidate)
            {
                continue;
            }

            var actor = ResolveActor(candidate);
            if (actor == elior)
            {
                eliorContacts.Add(candidate);
            }
            else if (actor == sim)
            {
                simContacts.Add(candidate);
            }
        }

        EvaluateRequirements();
    }

    PlayerOrchestrator ResolveActor(Collider2D collider)
    {
        if (!collider)
        {
            return null;
        }

        var actor = collider.GetComponent<PlayerOrchestrator>();
        if (actor)
        {
            return actor;
        }

        actor = collider.GetComponentInParent<PlayerOrchestrator>();
        if (actor)
        {
            return actor;
        }

        return collider.GetComponentInChildren<PlayerOrchestrator>();
    }

    void EvaluateRequirements()
    {
        PruneInvalidContacts(eliorContacts);
        PruneInvalidContacts(simContacts);

        bool ready = elior && sim && eliorContacts.Count > 0 && simContacts.Count > 0;
        if (ready && requireSplitState)
        {
            if (partyController)
            {
                ready &= !partyController.IsMerged;
            }
            else
            {
                ready = false;
            }
        }

        if (ready == requirementsSatisfied)
        {
            return;
        }

        requirementsSatisfied = ready;

        if (requirementsSatisfied)
        {
            UpdatePuzzleFlag(true);
            OnBothCharactersInside?.Invoke();

            if (autoLoadScene)
            {
                TriggerSceneLoad();
            }
        }
        else
        {
            if (pendingLoadRoutine != null)
            {
                StopCoroutine(pendingLoadRoutine);
                pendingLoadRoutine = null;
            }

            if (clearFlagWhenCharactersLeave)
            {
                UpdatePuzzleFlag(false);
            }

            sceneLoadQueued = false;
            OnRequirementsLost?.Invoke();
        }
    }

    void PruneInvalidContacts(HashSet<Collider2D> set)
    {
        set.RemoveWhere(c => !c || !c.enabled || !c.gameObject.activeInHierarchy);
    }

    void UpdatePuzzleFlag(bool value)
    {
        if (string.IsNullOrEmpty(readyFlagName))
        {
            return;
        }

        if (!puzzleManager)
        {
            puzzleManager = FindFirstObjectByType<PuzzleStateManager>();
        }

        if (!puzzleManager)
        {
            return;
        }

        puzzleManager.SetFlagState(readyFlagName, value);
        flagRaised = value;
    }

    /// <summary>
    /// Manually request a scene transition. This is useful if Auto Load Scene is disabled
    /// and the level exit is triggered via a separate interaction (e.g. a button press).
    /// </summary>
    public void TriggerSceneLoad()
    {
        if (sceneLoadQueued || !requirementsSatisfied)
        {
            return;
        }

        pendingLoadRoutine = StartCoroutine(BeginSceneLoadRoutine());
    }

    IEnumerator BeginSceneLoadRoutine()
    {
        sceneLoadQueued = true;

        float elapsed = 0f;
        while (loadDelay > 0f && elapsed < loadDelay)
        {
            if (!requirementsSatisfied)
            {
                sceneLoadQueued = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!requirementsSatisfied)
        {
            sceneLoadQueued = false;
            yield break;
        }

        OnSceneLoadStarted?.Invoke();
        LoadTargetScene();
    }

    void LoadTargetScene()
    {
        if (!requirementsSatisfied)
        {
            sceneLoadQueued = false;
            return;
        }

        var currentScene = SceneManager.GetActiveScene();
        string currentSceneName = currentScene.name;
        string targetSceneName = null;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            targetSceneName = nextSceneName;
            // Level progress'i kaydet
            SaveLevelProgress(currentSceneName, targetSceneName);
            SceneManager.LoadScene(targetSceneName);
            return;
        }

        int targetIndex = currentScene.buildIndex + buildIndexOffset;
        if (targetIndex < 0 || targetIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[CoopLevelExit] Target build index {targetIndex} is outside the range of configured scenes.", this);
            sceneLoadQueued = false;
            return;
        }

        // Build index'ten scene adını al
        var targetScenePath = SceneUtility.GetScenePathByBuildIndex(targetIndex);
        if (!string.IsNullOrEmpty(targetScenePath))
        {
            var sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(targetScenePath);
            targetSceneName = sceneNameFromPath;
        }

        // Level progress'i kaydet
        SaveLevelProgress(currentSceneName, targetSceneName);
        SceneManager.LoadScene(targetIndex);
    }

    void SaveLevelProgress(string currentSceneName, string nextSceneName)
    {
        if (LevelProgressSaveManager.Instance == null)
        {
            return;
        }

        // Mevcut seviyeyi tamamlandı olarak işaretle ve bir sonraki seviyeyi aç
        LevelProgressSaveManager.Instance.CompleteLevel(currentSceneName, nextSceneName);
    }
}
