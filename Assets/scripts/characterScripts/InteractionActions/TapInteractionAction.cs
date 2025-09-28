using UnityEngine;

[CreateAssetMenu(menuName = "Interactions/Tap Etkilesimi", fileName = "TapInteractionAction")]
public class TapInteractionAction : InteractionAction
{
    public override void OnComplete(InteractionContext context)
    {
        if (context.Interactable)
        {
            context.Interactable.ForceToggleState(false);
        }
    }
}
