using UnityEngine;

[DisallowMultipleComponent]
public class PlayerOrchestrator : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    AbilityController abilities;
    LocomotionMotor2D motor;
    AnimationStateSync animSync;

    void Awake()
    {
        input    = GetComponent<InputAdapter>();
        sensors  = GetComponent<Sensors2D>();
        abilities= GetComponent<AbilityController>();
        motor    = GetComponent<LocomotionMotor2D>();
        animSync = GetComponent<AnimationStateSync>();
    }

    void Update()
    {
        sensors?.Sample();
        abilities?.Tick(Time.deltaTime);
    }

    void FixedUpdate() => motor?.PhysicsStep(Time.fixedDeltaTime);
    void LateUpdate()  => animSync?.LateSync();
}
