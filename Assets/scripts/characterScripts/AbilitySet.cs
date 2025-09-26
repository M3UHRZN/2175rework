using System;
using UnityEngine;

[CreateAssetMenu(menuName = "2175/Ability Set", fileName = "AbilitySet")]
public class AbilitySet : ScriptableObject
{
    [Header("Movement")]
    public bool canMove = true;
    public bool canJump = true;
    public bool canWallSlide = true;
    public bool canWallClimb = true;
    public bool canWallJump = true;
    public bool canClimb = true;

    [Header("Interaction")]
    public bool canInteract = true;
    public bool canUsePanels = true;
    public bool canSupplyPower = false;
    public bool canRepair = false;

    [Header("Meta")]
    public bool canMerge = true;
    public bool canSwitchCharacter = true;
}

[Serializable]
public struct AbilitySnapshot
{
    public bool canMove;
    public bool canJump;
    public bool canWallSlide;
    public bool canWallClimb;
    public bool canWallJump;
    public bool canClimb;
    public bool canInteract;
    public bool canUsePanels;
    public bool canSupplyPower;
    public bool canRepair;
    public bool canMerge;
    public bool canSwitchCharacter;

    public static AbilitySnapshot AllEnabled => new AbilitySnapshot
    {
        canMove = true,
        canJump = true,
        canWallSlide = true,
        canWallClimb = true,
        canWallJump = true,
        canClimb = true,
        canInteract = true,
        canUsePanels = true,
        canSupplyPower = true,
        canRepair = true,
        canMerge = true,
        canSwitchCharacter = true
    };

    public static AbilitySnapshot FromSet(AbilitySet set)
    {
        var snapshot = new AbilitySnapshot();
        snapshot.Apply(set);
        return snapshot;
    }

    public void Apply(AbilitySet set)
    {
        if (!set) return;
        canMove = set.canMove;
        canJump = set.canJump;
        canWallSlide = set.canWallSlide;
        canWallClimb = set.canWallClimb;
        canWallJump = set.canWallJump;
        canClimb = set.canClimb;
        canInteract = set.canInteract;
        canUsePanels = set.canUsePanels;
        canSupplyPower = set.canSupplyPower;
        canRepair = set.canRepair;
        canMerge = set.canMerge;
        canSwitchCharacter = set.canSwitchCharacter;
    }
}

[DisallowMultipleComponent]
public class AbilityLoadout : MonoBehaviour
{
    [Tooltip("Abilities used when the agent is split and controllable on its own.")]
    public AbilitySet splitAbilities;

    [Tooltip("Abilities applied when this agent is inside the merged form.")]
    public AbilitySet mergedAbilities;

    [NonSerialized]
    AbilitySnapshot activeSnapshot;

    public AbilitySnapshot ActiveSnapshot => activeSnapshot;

    void Awake()
    {
        ApplyMergedState(false);
    }

    public void Initialise(bool merged)
    {
        ApplyMergedState(merged);
    }

    public void ApplyMergedState(bool merged)
    {
        if (merged && mergedAbilities)
        {
            activeSnapshot = AbilitySnapshot.FromSet(mergedAbilities);
        }
        else if (!merged && splitAbilities)
        {
            activeSnapshot = AbilitySnapshot.FromSet(splitAbilities);
        }
        else if (mergedAbilities)
        {
            activeSnapshot = AbilitySnapshot.FromSet(mergedAbilities);
        }
        else if (splitAbilities)
        {
            activeSnapshot = AbilitySnapshot.FromSet(splitAbilities);
        }
        else
        {
            activeSnapshot = AbilitySnapshot.AllEnabled;
        }
    }

    public void OverrideSnapshot(AbilitySnapshot snapshot)
    {
        activeSnapshot = snapshot;
    }
}
