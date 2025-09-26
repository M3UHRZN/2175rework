using UnityEngine;

[DisallowMultipleComponent]
public class InteractionController : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        sensors = GetComponent<Sensors2D>();
    }

    public void Tick(float dt)
    {
        if (input.InteractPressed && sensors.nearestInteractable)
            sensors.nearestInteractable.Activate();
    }
}
