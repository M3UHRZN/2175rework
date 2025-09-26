using UnityEngine;
using System;

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
