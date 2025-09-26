using UnityEngine;

[DisallowMultipleComponent]
public class JumpController : MonoBehaviour
{
    public JumpConfig jumpCfg;
    public MovementConfig moveCfg;

    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;
    AbilityRuntime abilities;

    float coyoteMsLeft;
    float bufferMsLeft;

    void Awake()
    {
        input = GetComponent<InputAdapter>();
        sensors = GetComponent<Sensors2D>();
        motor = GetComponent<LocomotionMotor2D>();
        fsm = GetComponent<PlayerStateMachine>();
        abilities = GetComponent<AbilityRuntime>();
    }

    public void Tick(float dt)
    {
        if (abilities && !abilities.CanJump)
        {
            coyoteMsLeft = 0f;
            bufferMsLeft = 0f;
            return;
        }

        // coyote
        if (sensors.isGrounded) coyoteMsLeft = jumpCfg ? jumpCfg.coyoteMs : 150f;
        else coyoteMsLeft = Mathf.Max(0f, coyoteMsLeft - dt * 1000f);

        // buffer
        if (input.JumpPressed) bufferMsLeft = jumpCfg ? jumpCfg.bufferMs : 150f;
        else bufferMsLeft = Mathf.Max(0f, bufferMsLeft - dt * 1000f);


        // Jump apply
        bool canJump = sensors.isGrounded || coyoteMsLeft > 0f;
        if (bufferMsLeft > 0f && canJump)
        {
            float g = Mathf.Abs(Physics2D.gravity.y * (moveCfg ? moveCfg.gravityScale : 3.5f));
            float vShort = Mathf.Sqrt(2f * g * (jumpCfg ? jumpCfg.shortJumpHeight : 1.25f));
            float vFull  = Mathf.Sqrt(2f * g * (jumpCfg ? jumpCfg.fullJumpHeight : 2.6f));
            float v = input.JumpHeld ? vFull : vShort;

            motor.RequestJump(v, cutEnabled: true);
            bufferMsLeft = 0f;
            coyoteMsLeft = 0f;
            
            // Sadece daha yüksek öncelikli state'e geçiş yapabilir
            if (fsm.CanTransitionTo(PlayerStateMachine.LocoState.JumpRise))
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpRise, "Jump");
            }
        }
    }
}
