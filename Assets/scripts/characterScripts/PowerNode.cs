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
            interactable.EtkilesimBasladi.AddListener(HandleStart);
            interactable.EtkilesimTamamlandi.AddListener(HandleComplete);
            interactable.EtkilesimIptalEdildi.AddListener(HandleCancel);
        }
    }

    void OnDestroy()
    {
        if (!interactable)
            return;
        interactable.EtkilesimBasladi.RemoveListener(HandleStart);
        interactable.EtkilesimTamamlandi.RemoveListener(HandleComplete);
        interactable.EtkilesimIptalEdildi.RemoveListener(HandleCancel);
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

        powered = interactable.ToggleState;
        OnPowerChanged?.Invoke(powered);
    }

    void HandleCancel(InteractionController controller)
    {
        if (!controller)
            return;
        if (interactable.ActionMode == InteractionActionMode.Instant)
            return;

        if (powered)
        {
            powered = false;
            OnPowerChanged?.Invoke(false);
        }
    }

    public bool IsPowered => powered;
}
