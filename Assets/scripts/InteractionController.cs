using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    AbilityRuntime abilities;

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        sensors = GetComponent<Sensors2D>();
        abilities = GetComponent<AbilityRuntime>();
    }

    public void Tick(float dt)
    {
        if (abilities && !abilities.CanInteract)
            return;

        if (input.InteractPressed && sensors.nearestInteractable)
            sensors.nearestInteractable.Activate();
    }
}
