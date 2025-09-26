using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-200)]
public class InputAdapter : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference move;             // Vector2
    public InputActionReference jump;             // Button
    public InputActionReference interact;         // Button (optional)
    public InputActionReference flashlightToggle; // Button

    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public bool  JumpHeld { get; private set; }
    public bool  JumpPressed { get; private set; }
    public bool  InteractPressed { get; private set; }
    public bool  FlashlightTogglePressed { get; private set; }

    void OnEnable()
    {
        move?.action?.Enable();
        jump?.action?.Enable();
        interact?.action?.Enable();
        flashlightToggle?.action?.Enable();
    }
    void OnDisable()
    {
        move?.action?.Disable();
        jump?.action?.Disable();
        interact?.action?.Disable();
        flashlightToggle?.action?.Disable();
    }

    void Update()
    {
        Collect();
    }

    void LateUpdate()
    {
        ClearFrameEdges();
    }

    public void Collect()
    {
        Vector2 mv = move ? move.action.ReadValue<Vector2>() : Vector2.zero;
        MoveX = Mathf.Clamp(mv.x, -1f, 1f);
        MoveY = Mathf.Clamp(mv.y, -1f, 1f);
        JumpHeld = jump && jump.action.IsPressed();
        JumpPressed = jump && jump.action.WasPressedThisFrame();
        InteractPressed = interact && interact.action.WasPressedThisFrame();
        FlashlightTogglePressed = flashlightToggle && flashlightToggle.action.WasPressedThisFrame();
    }

    public void ClearFrameEdges()
    {
        JumpPressed = false;
        InteractPressed = false;
        FlashlightTogglePressed = false;
    }
}
