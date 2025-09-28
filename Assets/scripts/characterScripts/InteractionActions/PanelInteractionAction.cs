using UnityEngine;

[CreateAssetMenu(menuName = "Interactions/Panel Etkilesimi", fileName = "PanelInteractionAction")]
public class PanelInteractionAction : HoldInteractionAction
{
    public override InteractionAbilityRequirement RequiredAbility => InteractionAbilityRequirement.Panel;

    public override bool LocksMovement => true;
}
