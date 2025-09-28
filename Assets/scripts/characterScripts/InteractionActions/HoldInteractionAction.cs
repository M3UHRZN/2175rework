using UnityEngine;

[CreateAssetMenu(menuName = "Interactions/Basma-Tutma Etkilesimi", fileName = "HoldInteractionAction")]
public class HoldInteractionAction : InteractionAction
{
    public override InteractionActionMode Mode => InteractionActionMode.Hold;

    public override void OnComplete(InteractionContext context)
    {
        if (context.Interactable)
        {
            context.Interactable.ForceToggleState(true);
        }
    }

    public override void OnCancel(InteractionContext context)
    {
        if (context.Interactable)
        {
            context.Interactable.ForceToggleState(false);
        }
    }
}
