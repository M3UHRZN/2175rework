using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class PowerNode : MonoBehaviour
{
    public Interactable interactable;
    public UnityEvent<bool> OnPowerChanged;

    bool powered;

    void OnEnable()
    {
        ResolveInteractable();
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void ResolveInteractable()
    {
        if (!interactable)
            interactable = GetComponent<Interactable>();
    }

    void Subscribe()
    {
        if (!interactable)
            return;

        interactable.EtkilesimBasladi.AddListener(HandleStart);
        interactable.EtkilesimTamamlandi.AddListener(HandleComplete);
        interactable.EtkilesimIptalEdildi.AddListener(HandleCancel);
    }

    void Unsubscribe()
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

        powered = interactable ? interactable.ToggleState : !powered;
        OnPowerChanged?.Invoke(powered);
    }

    void HandleCancel(InteractionController controller)
    {
        if (!powered)
            return;

        powered = false;
        OnPowerChanged?.Invoke(false);
    }

    public bool IsPowered => powered;
}
