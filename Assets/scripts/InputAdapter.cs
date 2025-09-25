using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InputAdapter : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference move;     // Vector2
    public InputActionReference jump;     // Button
    public InputActionReference interact; // Button (optional)

    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public bool  JumpHeld { get; private set; }
    public bool  JumpPressed { get; private set; }
    public bool  InteractPressed { get; private set; }

    void OnEnable()
    {
        move?.action?.Enable();
        jump?.action?.Enable();
        interact?.action?.Enable();
    }
    void OnDisable()
    {
        move?.action?.Disable();
        jump?.action?.Disable();
        interact?.action?.Disable();
    }

    public void Collect()
    {
        Vector2 mv = move ? move.action.ReadValue<Vector2>() : Vector2.zero;
        MoveX = Mathf.Clamp(mv.x, -1f, 1f);
        MoveY = Mathf.Clamp(mv.y, -1f, 1f);
        JumpHeld = jump && jump.action.IsPressed();
        JumpPressed = jump && jump.action.WasPressedThisFrame();
        InteractPressed = interact && interact.action.WasPressedThisFrame();
    }

    public void ClearFrameEdges()
    {
        JumpPressed = false;
        InteractPressed = false;
    }
}
