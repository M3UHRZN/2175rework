using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PowerNode : MonoBehaviour
{
    public Interactable interactable;
    public UnityEvent<bool> OnPowerChanged;

    bool powered;

    void Awake()
    {
        if (!interactable)
            interactable = GetComponent<Interactable>();

        if (interactable)
        {
            interactable.OnInteractStart.AddListener(HandleStart);
            interactable.OnInteractComplete.AddListener(HandleComplete);
            interactable.OnInteractCancel.AddListener(HandleCancel);
        }
    }

    void OnDestroy()
    {
        if (!interactable)
            return;
        interactable.OnInteractStart.RemoveListener(HandleStart);
        interactable.OnInteractComplete.RemoveListener(HandleComplete);
        interactable.OnInteractCancel.RemoveListener(HandleCancel);
    }

    void HandleStart(InteractionController controller)
    {
        if (!controller)
            return;
        if (!controller.CurrentAbilities.canSupplyPower)
        {
            controller.ForceCancelInteraction();
        }
    }

    void HandleComplete(InteractionController controller)
    {
        if (!controller || !controller.CurrentAbilities.canSupplyPower)
            return;

        powered = interactable.IsActivated;
        OnPowerChanged?.Invoke(powered);
    }

    void HandleCancel(InteractionController controller)
    {
        if (!controller)
            return;
        if (interactable.interactionType == InteractionType.Toggle)
            return;

        if (powered)
        {
            powered = false;
            OnPowerChanged?.Invoke(false);
        }
    }

    public bool IsPowered => powered;
}
