using UnityEngine;

[DisallowMultipleComponent]
public class AbilityRuntime : MonoBehaviour
{
    [System.Serializable]
    struct AbilitySnapshot
    {
        public bool canJump;
        public bool canWallJump;
        public bool canClimb;
        public bool canSupplyPower;
        public bool canUsePanels;
        public bool canRepair;
        public bool canMerge;
        public bool canSwitchCharacter;
    }

    [Header("Başlangıç Seti")]
    [Tooltip("Oyun başında uygulanacak AbilitySet referansı.")]
    [SerializeField] AbilitySet initialAbilitySet;

    [Tooltip("Awake sırasında initialAbilitySet otomatik olarak uygulanır.")]
    [SerializeField] bool applyInitialOnAwake = true;

    [Tooltip("Set değişimleri konsola loglansın mı?")]
    [SerializeField] bool logChanges = false;

    [Header("Anlık Durum (salt okunur)")]
    [SerializeField] AbilitySnapshot runtimeFlags;

    AbilitySet currentSet;

    void Awake()
    {
        if (applyInitialOnAwake)
        {
            ApplyAbilitySet(initialAbilitySet, silent: true);
        }
        else if (currentSet == null)
        {
            ResetToDefaults();
        }
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (initialAbilitySet)
                ApplyAbilitySet(initialAbilitySet, silent: true);
            else
                ResetToDefaults();
        }
#endif
    }

    public AbilitySet CurrentSet => currentSet;

    public bool CanJump            => runtimeFlags.canJump;
    public bool CanWallJump        => runtimeFlags.canWallJump;
    public bool CanClimb           => runtimeFlags.canClimb;
    public bool CanSupplyPower     => runtimeFlags.canSupplyPower;
    public bool CanUsePanels       => runtimeFlags.canUsePanels;
    public bool CanRepair          => runtimeFlags.canRepair;
    public bool CanMerge           => runtimeFlags.canMerge;
    public bool CanSwitchCharacter => runtimeFlags.canSwitchCharacter;
    public bool CanInteract        => runtimeFlags.canUsePanels || runtimeFlags.canSupplyPower || runtimeFlags.canRepair;

    public void ApplyAbilitySet(AbilitySet set) => ApplyAbilitySet(set, silent: false);

    public void ApplyAbilitySet(AbilitySet set, bool silent)
    {
        currentSet = set;

        if (set)
        {
            runtimeFlags.canJump = set.canJump;
            runtimeFlags.canWallJump = set.canWallJump;
            runtimeFlags.canClimb = set.canClimb;
            runtimeFlags.canSupplyPower = set.canSupplyPower;
            runtimeFlags.canUsePanels = set.canUsePanels;
            runtimeFlags.canRepair = set.canRepair;
            runtimeFlags.canMerge = set.canMerge;
            runtimeFlags.canSwitchCharacter = set.canSwitchCharacter;
        }
        else
        {
            ResetToDefaults();
        }

        if (logChanges && !silent)
        {
            Debug.Log($"[AbilityRuntime] {name} set -> {(set ? set.name : "(default)")}", this);
        }
    }

    public void OverrideFlags(bool canJump, bool canWallJump, bool canClimb,
        bool canSupplyPower, bool canUsePanels, bool canRepair, bool canMerge, bool canSwitchCharacter)
    {
        runtimeFlags.canJump = canJump;
        runtimeFlags.canWallJump = canWallJump;
        runtimeFlags.canClimb = canClimb;
        runtimeFlags.canSupplyPower = canSupplyPower;
        runtimeFlags.canUsePanels = canUsePanels;
        runtimeFlags.canRepair = canRepair;
        runtimeFlags.canMerge = canMerge;
        runtimeFlags.canSwitchCharacter = canSwitchCharacter;
        currentSet = null;

        if (logChanges)
        {
            Debug.Log($"[AbilityRuntime] {name} flags override", this);
        }
    }

    public void ResetToDefaults()
    {
        runtimeFlags.canJump = true;
        runtimeFlags.canWallJump = true;
        runtimeFlags.canClimb = true;
        runtimeFlags.canSupplyPower = true;
        runtimeFlags.canUsePanels = true;
        runtimeFlags.canRepair = true;
        runtimeFlags.canMerge = true;
        runtimeFlags.canSwitchCharacter = true;
    }
}
