using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class RepairNode : MonoBehaviour
{
    public Interactable interactable;
    public UnityEvent<bool> OnRepairedChanged;

    bool repaired;

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
        if (!controller.CurrentAbilities.canRepair)
        {
            controller.ForceCancelInteraction();
        }
    }

    void HandleComplete(InteractionController controller)
    {
        if (!controller || !controller.CurrentAbilities.canRepair)
            return;

        repaired = true;
        OnRepairedChanged?.Invoke(true);
    }

    void HandleCancel(InteractionController controller)
    {
        if (!controller)
            return;
        if (!controller.CurrentAbilities.canRepair)
            return;

        if (!repaired)
            OnRepairedChanged?.Invoke(false);
    }

    public bool IsRepaired => repaired;
}
