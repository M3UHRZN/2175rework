using UnityEngine;

[DisallowMultipleComponent]
public class PlayerOrchestrator : MonoBehaviour
{
    InputAdapter input;
    Sensors2D sensors;
    AbilityController abilities;
    PlayerStateHooks stateHooks;
    LocomotionMotor2D motor;
    AnimationStateSync animSync;

    [Tooltip("Automatically clear input edge states at the end of Update().")]
    public bool autoClearInput = true;

    void Awake()
    {
        input    = GetComponent<InputAdapter>();
        sensors  = GetComponent<Sensors2D>();
        abilities= GetComponent<AbilityController>();
        stateHooks = GetComponent<PlayerStateHooks>();
        motor    = GetComponent<LocomotionMotor2D>();
        animSync = GetComponent<AnimationStateSync>();
    }

    void Update()
    {
        input?.Collect();
        sensors?.Sample();
        abilities?.Tick(Time.deltaTime);
        stateHooks?.Tick(Time.deltaTime);
        if (autoClearInput)
            input?.ClearFrameEdges();
    }

    void FixedUpdate() => motor?.PhysicsStep(Time.fixedDeltaTime);
    void LateUpdate()  => animSync?.LateSync();
}
