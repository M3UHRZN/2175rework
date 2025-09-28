using UnityEngine;

[CreateAssetMenu(menuName = "Interactions/Anahtar Etkilesimi", fileName = "ToggleInteractionAction")]
public class ToggleInteractionAction : InteractionAction
{
    public override void OnComplete(InteractionContext context)
    {
        if (context.Interactable)
        {
            bool nextState = !context.Interactable.ToggleState;
            context.Interactable.ForceToggleState(nextState);
        }
    }
}
