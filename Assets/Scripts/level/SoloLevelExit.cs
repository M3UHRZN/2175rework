using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles solo level transitions that require Elior to stand inside a trigger volume
/// before advancing to the next scene. Used for the first level where only Elior is present.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SoloLevelExit : MonoBehaviour
{
    private const int OverlapBufferSize = 16;

    [Header("Character")]
    [Tooltip("Reference to Elior's orchestrator component.")]
    [SerializeField] PlayerOrchestrator elior;

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
    [Tooltip("Optional flag name pushed to the PuzzleStateManager while Elior stands inside the exit.")]
    [SerializeField] string readyFlagName;

    [Tooltip("Clear the puzzle flag automatically when Elior leaves the exit volume.")]
    [SerializeField] bool clearFlagWhenCharacterLeaves = true;

    [Header("Events")]
    [Tooltip("Invoked the first frame Elior satisfies the requirements (enters the trigger volume).")]
    public UnityEvent OnCharacterInside;

    [Tooltip("Invoked when the requirements are no longer satisfied (Elior leaves the trigger volume).")]
    public UnityEvent OnRequirementsLost;

    [Tooltip("Invoked right before the scene loading routine runs (after the optional delay).")]
    public UnityEvent OnSceneLoadStarted;

    readonly HashSet<Collider2D> eliorContacts = new HashSet<Collider2D>();
    readonly Collider2D[] overlapBuffer = new Collider2D[OverlapBufferSize];

    Collider2D triggerCollider;
    PuzzleStateManager puzzleManager;
    Coroutine pendingLoadRoutine;
    bool requirementsSatisfied;
    bool sceneLoadQueued;
    bool flagRaised;

    void Reset()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider)
        {
            triggerCollider.isTrigger = true;
        }

        AutoAssignCharacter();
    }

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[SoloLevelExit] Collider on {name} was not set as a trigger. Enabling trigger mode automatically.", this);
            triggerCollider.isTrigger = true;
        }

        AutoAssignCharacter();
        puzzleManager = FindFirstObjectByType<PuzzleStateManager>();
        UpdatePuzzleFlag(false);
    }

    void OnEnable()
    {
        RefreshCurrentOverlaps();
    }

    void OnDisable()
    {
        if (pendingLoadRoutine != null)
        {
            StopCoroutine(pendingLoadRoutine);
            pendingLoadRoutine = null;
        }

        sceneLoadQueued = false;
        requirementsSatisfied = false;
        eliorContacts.Clear();

        if (flagRaised && clearFlagWhenCharacterLeaves)
        {
            UpdatePuzzleFlag(false);
        }
    }

    void AutoAssignCharacter()
    {
        if (!elior)
        {
            elior = FindFirstObjectByType<PlayerOrchestrator>();
            if (!elior && Application.isPlaying)
            {
                Debug.LogWarning("[SoloLevelExit] Elior reference is missing. Assign Elior's PlayerOrchestrator in the inspector.", this);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var actor = ResolveActor(other);
        if (!actor || actor != elior)
        {
            return;
        }

        if (eliorContacts.Add(other))
        {
            EvaluateRequirements();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var actor = ResolveActor(other);
        if (!actor || actor != elior)
        {
            return;
        }

        if (eliorContacts.Remove(other))
        {
            EvaluateRequirements();
        }
    }

    void RefreshCurrentOverlaps()
    {
        eliorContacts.Clear();

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

        bool ready = elior && eliorContacts.Count > 0;

        if (ready == requirementsSatisfied)
        {
            return;
        }

        requirementsSatisfied = ready;

        if (requirementsSatisfied)
        {
            UpdatePuzzleFlag(true);
            OnCharacterInside?.Invoke();

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

            if (clearFlagWhenCharacterLeaves)
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

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        var currentScene = SceneManager.GetActiveScene();
        int targetIndex = currentScene.buildIndex + buildIndexOffset;
        if (targetIndex < 0 || targetIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"[SoloLevelExit] Target build index {targetIndex} is outside the range of configured scenes.", this);
            sceneLoadQueued = false;
            return;
        }

        SceneManager.LoadScene(targetIndex);
    }
}
