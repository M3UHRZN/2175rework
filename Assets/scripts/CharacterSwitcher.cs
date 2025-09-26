using UnityEngine;

[DisallowMultipleComponent]
public class CharacterSwitcher : MonoBehaviour, FlashlightController.IActiveCharacterGate
{
    public enum AgentState
    {
        Merged,
        Elior,
        Sim
    }

    [Header("Ajan Referansları")]
    [Tooltip("Elior'un kök GameObject'i. Boş bırakılırsa bu nesne kullanılır.")]
    [SerializeField] GameObject eliorRoot;

    [Tooltip("Sim'in kök GameObject'i.")]
    [SerializeField] GameObject simRoot;

    [Tooltip("Elior sensör bileşeni (yer kontrolü için).")]
    [SerializeField] Sensors2D eliorSensors;

    [Tooltip("Sim sensör bileşeni (yer kontrolü için).")]
    [SerializeField] Sensors2D simSensors;

    [Tooltip("Elior'un AbilityRuntime bileşeni.")]
    [SerializeField] AbilityRuntime eliorAbilities;

    [Tooltip("Sim'in AbilityRuntime bileşeni.")]
    [SerializeField] AbilityRuntime simAbilities;

    [Tooltip("Merge/Split işlemini yönetecek MergeController.")]
    [SerializeField] MergeController mergeController;

    [Tooltip("Girişleri okuyacak InputAdapter (boşsa aynı nesneden alınır).")]
    [SerializeField] InputAdapter input;

    [Header("Başlangıç Durumu")]
    [Tooltip("Sahne yüklendiğinde hangi ajan aktif sayılacak?")]
    [SerializeField] AgentState initialAgentState = AgentState.Merged;

    [Header("Yetenek Setleri")]
    [Tooltip("Merged modda Elior için uygulanacak yetenek seti.")]
    [SerializeField] AbilitySet mergedEliorAbilities;

    [Tooltip("Merged modda Sim için uygulanacak yetenek seti (boşsa paneller/güç kapatılır).")]
    [SerializeField] AbilitySet mergedSimAbilities;

    [Tooltip("Split modda Elior için uygulanacak yetenek seti.")]
    [SerializeField] AbilitySet splitEliorAbilities;

    [Tooltip("Split modda Sim için uygulanacak yetenek seti.")]
    [SerializeField] AbilitySet splitSimAbilities;

    [Header("Switch Ayarları")]
    [Tooltip("Karakter değişiminde uygulanacak cooldown süresi (saniye).")]
    [SerializeField] float switchCooldownSeconds = 0.35f;

    [Tooltip("Switch yalnızca yerdeyken çalışsın mı?")]
    [SerializeField] bool requireGroundedForSwitch = true;

    [Tooltip("Merge/Split yalnızca yere basarken tetiklensin mi?")]
    [SerializeField] bool requireGroundedForMergeToggle = true;

    [Tooltip("Split tamamlandığında varsayılan aktif ajan Sim olsun mu?")]
    [SerializeField] bool preferSimAfterSplit = true;

    [Tooltip("Time.timeScale <= 0 olduğunda girişleri yok say.")]
    [SerializeField] bool respectPauseState = true;

    [Header("Input Shim (opsiyonel)")]
    [Tooltip("InputAdapter aksiyonları tanımlı değilse harici tuş okumasını açar.")]
    [SerializeField] bool useInputShim = false;

    [Tooltip("InputShim açıkken ajan değişim tuşu.")]
    [SerializeField] KeyCode switchKey = KeyCode.Q;

    [Tooltip("InputShim açıkken merge toggle tuşu.")]
    [SerializeField] KeyCode mergeKey = KeyCode.R;

    [Header("Debug")]
    [Tooltip("TransitionLock aktifken ekranda bilgilendirici yazı göster.")]
    [SerializeField] bool showTransitionLabel = true;

    [Tooltip("Merge sırasında gösterilecek yazı.")]
    [SerializeField] string mergingLabel = "Merging...";

    [Tooltip("Split sırasında gösterilecek yazı.")]
    [SerializeField] string splittingLabel = "Splitting...";

    [Tooltip("OnGUI etiketi ekran pozisyonu.")]
    [SerializeField] Vector2 labelScreenPosition = new(20f, 20f);

    [Tooltip("OnGUI etiketi rengi.")]
    [SerializeField] Color labelColor = Color.white;

    [Tooltip("Durum değişimleri ve akışlar konsola loglansın mı?")]
    [SerializeField] bool logTransitions = true;

    public AgentState ActiveAgent => activeAgent;
    public bool TransitionLock => transitionLock;
    public bool IsSplit => activeAgent != AgentState.Merged;

    public event System.Action<AgentState> ActiveAgentChanged;

    AgentState activeAgent;
    AgentState pendingAgent;
    float switchCooldownTimer;
    bool transitionLock;
    string currentTransitionLabel;
    bool warnedMissingMergedSimSet;
    bool warnedMissingSplitSimSet;

    void Awake()
    {
        if (!eliorRoot)
            eliorRoot = gameObject;

        input ??= GetComponent<InputAdapter>();
        mergeController ??= GetComponent<MergeController>();
        eliorSensors ??= eliorRoot ? eliorRoot.GetComponent<Sensors2D>() : null;
        simSensors ??= simRoot ? simRoot.GetComponent<Sensors2D>() : null;
        eliorAbilities ??= eliorRoot ? eliorRoot.GetComponent<AbilityRuntime>() : null;
        simAbilities ??= simRoot ? simRoot.GetComponent<AbilityRuntime>() : null;

        activeAgent = initialAgentState;
        pendingAgent = activeAgent;

        if (mergeController)
        {
            mergeController.RegisterSwitcher(this);
        }

        ApplyAbilityConfiguration(activeAgent, silent: true);
    }

    void Update()
    {
        if (respectPauseState && Time.timeScale <= 0f)
        {
            return;
        }

        if (switchCooldownTimer > 0f)
        {
            switchCooldownTimer = Mathf.Max(0f, switchCooldownTimer - Time.deltaTime);
        }

        bool switchPressed = input && input.SwitchCharacterPressed;
        bool mergePressed = input && input.MergeTogglePressed;

        if (useInputShim)
        {
            switchPressed |= Input.GetKeyDown(switchKey);
            mergePressed  |= Input.GetKeyDown(mergeKey);
        }

        if (mergePressed)
        {
            HandleMergeToggle();
        }
        else if (switchPressed)
        {
            HandleSwitchRequest();
        }
    }

    void HandleSwitchRequest()
    {
        if (!IsSplit)
            return;

        if (transitionLock)
            return;

        if (switchCooldownTimer > 0f)
            return;

        var abilities = GetAbilitiesForAgent(activeAgent);
        if (abilities && !abilities.CanSwitchCharacter)
            return;

        if (requireGroundedForSwitch && !IsGrounded(activeAgent))
            return;

        AgentState target = activeAgent == AgentState.Elior ? AgentState.Sim : AgentState.Elior;
        SetActiveAgentInternal(target, raiseEvent: true);
        switchCooldownTimer = switchCooldownSeconds;

        if (logTransitions)
        {
            Debug.Log($"[CharacterSwitcher] Switch -> {target}", this);
        }
    }

    void HandleMergeToggle()
    {
        if (transitionLock)
            return;

        bool wantSplit = activeAgent == AgentState.Merged;

        if (!HasMergePermission(wantSplit))
            return;

        if (requireGroundedForMergeToggle && !GroundRequirementSatisfied(wantSplit))
            return;

        if (!mergeController)
            return;

        if (wantSplit)
        {
            AgentState target = preferSimAfterSplit ? AgentState.Sim : AgentState.Elior;
            if (mergeController.BeginSplit())
            {
                BeginTransition(target, splittingLabel);
                if (logTransitions)
                    Debug.Log("[CharacterSwitcher] Split started", this);
            }
        }
        else
        {
            if (mergeController.BeginMerge())
            {
                BeginTransition(AgentState.Merged, mergingLabel);
                if (logTransitions)
                    Debug.Log("[CharacterSwitcher] Merge started", this);
            }
        }
    }

    void BeginTransition(AgentState target, string label)
    {
        pendingAgent = target;
        SetTransitionLock(true, label);
    }

    public void SetTransitionLock(bool locked, string label = null)
    {
        transitionLock = locked;
        currentTransitionLabel = locked ? label : null;
        if (!locked)
        {
            switchCooldownTimer = 0f;
        }
    }

    void SetActiveAgentInternal(AgentState newState, bool raiseEvent)
    {
        if (activeAgent == newState)
            return;

        activeAgent = newState;
        ApplyAbilityConfiguration(activeAgent, silent: !logTransitions);
        pendingAgent = activeAgent;

        if (raiseEvent)
            ActiveAgentChanged?.Invoke(activeAgent);
    }

    bool HasMergePermission(bool wantSplit)
    {
        if (wantSplit)
        {
            var abilities = eliorAbilities;
            if (abilities && !abilities.CanMerge)
                return false;
        }
        else
        {
            bool eliorOk = !eliorAbilities || eliorAbilities.CanMerge;
            bool simOk   = !simAbilities   || simAbilities.CanMerge;
            if (!eliorOk || !simOk)
                return false;
        }

        return true;
    }

    bool GroundRequirementSatisfied(bool wantSplit)
    {
        if (!requireGroundedForMergeToggle)
            return true;

        if (wantSplit)
            return IsGrounded(AgentState.Elior);

        bool eliorGround = !eliorSensors || eliorSensors.isGrounded;
        bool simGround   = !simSensors   || simSensors.isGrounded;
        return eliorGround && simGround;
    }

    bool IsGrounded(AgentState agent)
    {
        Sensors2D sensor = agent == AgentState.Sim ? simSensors : eliorSensors;
        return !sensor || sensor.isGrounded;
    }

    AbilityRuntime GetAbilitiesForAgent(AgentState agent)
    {
        return agent == AgentState.Sim ? simAbilities : eliorAbilities;
    }

    void ApplyAbilityConfiguration(AgentState state, bool silent)
    {
        switch (state)
        {
            case AgentState.Merged:
                ApplyAbilitySet(eliorAbilities, mergedEliorAbilities, splitEliorAbilities, silent);
                ApplyMergedSimAbilities(silent);
                break;
            case AgentState.Elior:
            case AgentState.Sim:
                ApplyAbilitySet(eliorAbilities, splitEliorAbilities, mergedEliorAbilities, silent);
                ApplyAbilitySet(simAbilities, splitSimAbilities, mergedSimAbilities, silent);
                break;
        }
    }

    void ApplyAbilitySet(AbilityRuntime runtime, AbilitySet preferred, AbilitySet fallback, bool silent)
    {
        if (!runtime)
            return;

        if (preferred)
        {
            runtime.ApplyAbilitySet(preferred, silent);
            return;
        }

        if (fallback)
        {
            runtime.ApplyAbilitySet(fallback, silent);
            return;
        }

        runtime.ApplyAbilitySet(null, silent);
    }

    void ApplyMergedSimAbilities(bool silent)
    {
        if (!simAbilities)
            return;

        if (mergedSimAbilities)
        {
            simAbilities.ApplyAbilitySet(mergedSimAbilities, silent);
            return;
        }

        if (!splitSimAbilities)
        {
            if (!warnedMissingSplitSimSet && logTransitions && !silent)
            {
                Debug.LogWarning("[CharacterSwitcher] splitSimAbilities atanmadı; varsayılan yeteneklerle devam ediliyor.", this);
                warnedMissingSplitSimSet = true;
            }

            simAbilities.ResetToDefaults();
            simAbilities.OverrideFlags(true, true, true, false, false, simAbilities.CanRepair, simAbilities.CanMerge, simAbilities.CanSwitchCharacter);
            return;
        }

        simAbilities.ApplyAbilitySet(splitSimAbilities, true);

        bool jump = simAbilities.CanJump;
        bool wallJump = simAbilities.CanWallJump;
        bool climb = simAbilities.CanClimb;
        bool repair = simAbilities.CanRepair;
        bool merge = simAbilities.CanMerge;
        bool switchChar = simAbilities.CanSwitchCharacter;

        simAbilities.OverrideFlags(jump, wallJump, climb, false, false, repair, merge, switchChar);

        if (!warnedMissingMergedSimSet && logTransitions && !silent)
        {
            Debug.LogWarning("[CharacterSwitcher] mergedSimAbilities atanmadı; Split setinden türetilip Sim'e özel yetenekler kapatıldı.", this);
            warnedMissingMergedSimSet = true;
        }
    }

    internal void NotifyTransitionCompleted(MergeController.TransitionKind kind)
    {
        switch (kind)
        {
            case MergeController.TransitionKind.Merge:
                SetActiveAgentInternal(AgentState.Merged, raiseEvent: true);
                if (logTransitions)
                    Debug.Log("[CharacterSwitcher] Merge completed", this);
                break;
            case MergeController.TransitionKind.Split:
                AgentState resolved = pendingAgent;
                if (resolved == AgentState.Merged)
                    resolved = preferSimAfterSplit ? AgentState.Sim : AgentState.Elior;
                SetActiveAgentInternal(resolved, raiseEvent: true);
                if (logTransitions)
                    Debug.Log($"[CharacterSwitcher] Split completed -> {resolved}", this);
                break;
        }

        SetTransitionLock(false);
    }

    internal void NotifyTransitionCancelled()
    {
        SetTransitionLock(false);
    }

    void OnGUI()
    {
        if (!showTransitionLabel || !transitionLock)
            return;

        if (string.IsNullOrEmpty(currentTransitionLabel))
            return;

        Color prev = GUI.color;
        GUI.color = labelColor;
        GUI.Label(new Rect(labelScreenPosition.x, labelScreenPosition.y, 200f, 24f), currentTransitionLabel);
        GUI.color = prev;
    }

    public bool IsCharacterActive(GameObject character)
    {
        if (!character)
            return false;

        if (activeAgent == AgentState.Merged)
            return IsTarget(character, eliorRoot);

        if (activeAgent == AgentState.Elior)
            return IsTarget(character, eliorRoot);

        return IsTarget(character, simRoot);
    }

    bool IsTarget(GameObject candidate, GameObject root)
    {
        if (!candidate || !root)
            return false;

        if (candidate == root)
            return true;

        return candidate.transform.IsChildOf(root.transform);
    }
}
