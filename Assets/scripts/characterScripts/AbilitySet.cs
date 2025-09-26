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

