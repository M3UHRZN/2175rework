using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class DualCharacterController : MonoBehaviour
{
    [Header("Characters")]
    public PlayerOrchestrator elior;
    public PlayerOrchestrator sim;

    [Header("Animation")]
    public AnimationFacade animationFacade;
    public string mergeTriggerName = "MergeTrigger";
    public string splitTriggerName = "SplitTrigger";

    [Header("Split Placement")]
    public Vector2 splitOffset = new Vector2(0.8f, 0f);
    public float splitProbeRadius = 0.3f;
    public LayerMask splitBlockMask;

    [Header("Timing")]
    public float switchCooldown = 0.35f;
    public float mergeInputCooldown = 0.45f;

    [Header("Events")]
    public UnityEvent<string> OnActiveCharacterChanged;
    public UnityEvent<bool> OnMergedStateChanged;

    PlayerOrchestrator active;
    InputAdapter eliorInput;
    InputAdapter simInput;
    AbilityLoadout eliorAbilities;
    AbilityLoadout simAbilities;

    bool isMerged = true;
    bool controlLocked = false;
    float switchCooldownTimer;
    float mergeCooldownTimer;

    readonly List<Vector2> splitSamples = new List<Vector2>();

    public PlayerOrchestrator Active => active;
    public bool IsMerged => isMerged;
    public bool ControlLocked => controlLocked;

    void Awake()
    {
        EnsureCharactersAssigned();

        if (!animationFacade && elior)
            animationFacade = elior.GetComponent<AnimationFacade>();

        if (elior)
        {
            eliorInput = elior.GetComponent<InputAdapter>();
            eliorAbilities = elior.GetComponent<AbilityLoadout>();
            elior.autoClearInput = false;
            if (splitBlockMask.value == 0)
            {
                var sensors = elior.GetComponent<Sensors2D>();
                if (sensors)
                    splitBlockMask = sensors.solidMask;
            }
        }
        if (sim)
        {
            simInput = sim.GetComponent<InputAdapter>();
            simAbilities = sim.GetComponent<AbilityLoadout>();
            sim.autoClearInput = false;
        }

        splitSamples.Add(splitOffset);
        splitSamples.Add(new Vector2(-splitOffset.x, splitOffset.y));
        splitSamples.Add(new Vector2(splitOffset.x, splitOffset.y + 0.5f));
        splitSamples.Add(new Vector2(-splitOffset.x, splitOffset.y + 0.5f));
        splitSamples.Add(Vector2.up * 0.5f);
    }

    void EnsureCharactersAssigned()
    {
        if (elior && sim)
            return;

        var candidates = new List<PlayerOrchestrator>();
        candidates.AddRange(GetComponentsInChildren<PlayerOrchestrator>(true));

        if (!elior || !sim)
        {
            foreach (var orchestrator in FindObjectsOfType<PlayerOrchestrator>(true))
            {
                if (!candidates.Contains(orchestrator))
                    candidates.Add(orchestrator);
            }
        }

        if (!elior)
        {
            foreach (var orchestrator in candidates)
            {
                if (!orchestrator)
                    continue;
                var name = orchestrator.name.ToLowerInvariant();
                if (name.Contains("elior"))
                {
                    elior = orchestrator;
                    break;
                }
            }
        }

        if (!sim)
        {
            foreach (var orchestrator in candidates)
            {
                if (!orchestrator || orchestrator == elior)
                    continue;
                var name = orchestrator.name.ToLowerInvariant();
                if (name.Contains("sim"))
                {
                    sim = orchestrator;
                    break;
                }
            }
        }

        if (!elior && candidates.Count > 0)
            elior = candidates[0];

        if (!sim)
        {
            for (int i = candidates.Count - 1; i >= 0; --i)
            {
                var candidate = candidates[i];
                if (candidate && candidate != elior)
                {
                    sim = candidate;
                    break;
                }
            }
        }
    }

    void Start()
    {
        ForceMergedState();
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (switchCooldownTimer > 0f)
            switchCooldownTimer = Mathf.Max(0f, switchCooldownTimer - dt);
        if (mergeCooldownTimer > 0f)
            mergeCooldownTimer = Mathf.Max(0f, mergeCooldownTimer - dt);

        if (controlLocked)
            return;

        HandleMetaInput();
    }

    void LateUpdate()
    {
        eliorInput?.ClearFrameEdges();
        simInput?.ClearFrameEdges();
    }

    void HandleMetaInput()
    {
        var source = active == elior ? eliorInput : simInput;
        if (!source)
            return;

        if (source.SwitchCharacterPressed)
            TrySwitchCharacter();

        if (source.MergeTogglePressed)
            ToggleMergeState();
    }

    void ForceMergedState()
    {
        isMerged = true;
        controlLocked = false;
        switchCooldownTimer = 0f;
        mergeCooldownTimer = 0f;

        if (sim)
            sim.gameObject.SetActive(false);

        if (eliorAbilities != null)
            eliorAbilities.Initialise(true);
        if (simAbilities != null)
            simAbilities.Initialise(true);

        ActivateCharacter(elior);
        OnMergedStateChanged?.Invoke(true);
    }

    void ActivateCharacter(PlayerOrchestrator target)
    {
        active = target;
        if (eliorInput)
            eliorInput.InputEnabled = target == elior;
        if (simInput)
            simInput.InputEnabled = target == sim && sim && sim.gameObject.activeSelf;

        OnActiveCharacterChanged?.Invoke(active == elior ? "Elior" : "Sim");
    }

    void TrySwitchCharacter()
    {
        if (isMerged || switchCooldownTimer > 0f)
            return;

        var sensors = active ? active.GetComponent<Sensors2D>() : null;
        if (sensors && !sensors.isGrounded)
            return;

        var loadout = active ? active.GetComponent<AbilityLoadout>() : null;
        if (loadout != null && !loadout.ActiveSnapshot.canSwitchCharacter)
            return;

        PlayerOrchestrator next = active == elior ? sim : elior;
        if (!next || (next == sim && !sim.gameObject.activeSelf))
            return;

        ActivateCharacter(next);
        switchCooldownTimer = switchCooldown;
    }

    void ToggleMergeState()
    {
        if (mergeCooldownTimer > 0f)
            return;

        if (isMerged)
            TrySplit();
        else
            TryMerge();
    }

    bool CanMergeAbility()
    {
        bool eliorCan = eliorAbilities == null || eliorAbilities.ActiveSnapshot.canMerge;
        bool simCan = simAbilities == null || simAbilities.ActiveSnapshot.canMerge;
        return eliorCan && simCan;
    }

    void TrySplit()
    {
        if (!CanMergeAbility() || !elior)
            return;
        if (!sim)
            return;

        Vector3 targetPos;
        if (!FindSplitPosition(out targetPos))
        {
            Debug.LogWarning("Split failed: could not find a safe spawn position for Sim.", this);
            return;
        }

        LockControl();
        sim.transform.position = targetPos;
        sim.gameObject.SetActive(true);
        if (simAbilities != null)
            simAbilities.ApplyMergedState(false);
        if (eliorAbilities != null)
            eliorAbilities.ApplyMergedState(false);

        TriggerAnimation(splitTriggerName);
        if (!animationFacade)
            FinishSplit();
    }

    void TryMerge()
    {
        if (!CanMergeAbility())
            return;
        if (!sim || !sim.gameObject.activeSelf)
            return;

        float distance = Vector2.Distance(elior.transform.position, sim.transform.position);
        if (distance > 3f)
            return;

        LockControl();
        TriggerAnimation(mergeTriggerName);
        if (!animationFacade)
            FinishMerge();
    }

    bool FindSplitPosition(out Vector3 position)
    {
        position = elior ? elior.transform.position : transform.position;
        if (!elior)
            return false;

        foreach (var offset in splitSamples)
        {
            Vector3 candidate = elior.transform.position + (Vector3)offset;
            if (!Physics2D.OverlapCircle(candidate, splitProbeRadius, splitBlockMask))
            {
                position = candidate;
                return true;
            }
        }

        return false;
    }

    void LockControl()
    {
        controlLocked = true;
        if (eliorInput) eliorInput.InputEnabled = false;
        if (simInput) simInput.InputEnabled = false;
    }

    void UnlockControl()
    {
        controlLocked = false;
        if (isMerged)
        {
            ActivateCharacter(elior);
        }
        else
        {
            ActivateCharacter(active == sim ? sim : elior);
        }
    }

    void TriggerAnimation(string trigger)
    {
        if (animationFacade)
            animationFacade.Trigger(trigger);
    }

    public void OnMergeAnimFinished()
    {
        FinishMerge();
    }

    public void OnSplitAnimFinished()
    {
        FinishSplit();
    }

    void FinishMerge()
    {
        isMerged = true;
        if (sim)
        {
            sim.transform.position = elior ? elior.transform.position : sim.transform.position;
            sim.gameObject.SetActive(false);
        }
        if (eliorAbilities != null)
            eliorAbilities.ApplyMergedState(true);
        if (simAbilities != null)
            simAbilities.ApplyMergedState(true);

        mergeCooldownTimer = mergeInputCooldown;
        switchCooldownTimer = switchCooldown;
        OnMergedStateChanged?.Invoke(true);
        UnlockControl();
    }

    void FinishSplit()
    {
        isMerged = false;
        if (eliorAbilities != null)
            eliorAbilities.ApplyMergedState(false);
        if (simAbilities != null)
            simAbilities.ApplyMergedState(false);
        mergeCooldownTimer = mergeInputCooldown;
        switchCooldownTimer = switchCooldown;
        OnMergedStateChanged?.Invoke(false);
        UnlockControl();
    }
}
