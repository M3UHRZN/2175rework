using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class InputAdapter : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference move;             // Vector2
    public InputActionReference jump;             // Button
    public InputActionReference interact;         // Button (optional)
    public InputActionReference flashlightToggle; // Button
    public InputActionReference switchCharacter;  // Button
    public InputActionReference mergeToggle;      // Button

    public float MoveX { get; private set; }
    public float MoveY { get; private set; }
    public bool  JumpHeld { get; private set; }
    public bool  JumpPressed { get; private set; }
    public bool  InteractPressed { get; private set; }
    public bool  FlashlightTogglePressed { get; private set; }
    public bool  SwitchCharacterPressed { get; private set; }
    public bool  MergeTogglePressed { get; private set; }
    public bool  InteractHeld { get; private set; }
    public bool  InteractReleased { get; private set; }
    public bool  InputEnabled { get; set; } = true;

    void OnEnable()
    {
        move?.action?.Enable();
        jump?.action?.Enable();
        interact?.action?.Enable();
        flashlightToggle?.action?.Enable();
        switchCharacter?.action?.Enable();
        mergeToggle?.action?.Enable();
    }
    void OnDisable()
    {
        move?.action?.Disable();
        jump?.action?.Disable();
        interact?.action?.Disable();
        flashlightToggle?.action?.Disable();
        switchCharacter?.action?.Disable();
        mergeToggle?.action?.Disable();
    }

    public void Collect()
    {
        if (!InputEnabled)
        {
            Debug.Log($"[Input] {gameObject.name} - Input disabled, clearing all inputs");
            MoveX = 0f;
            MoveY = 0f;
            JumpHeld = false;
            JumpPressed = false;
            InteractPressed = false;
            InteractHeld = false;
            InteractReleased = false;
            FlashlightTogglePressed = false;
            SwitchCharacterPressed = false;
            MergeTogglePressed = false;
            return;
        }

        Vector2 mv = move ? move.action.ReadValue<Vector2>() : Vector2.zero;
        MoveX = Mathf.Clamp(mv.x, -1f, 1f);
        MoveY = Mathf.Clamp(mv.y, -1f, 1f);
        
        // Debug: Input değerlerini kontrol et
        if (Mathf.Abs(MoveX) > 0.1f || Mathf.Abs(MoveY) > 0.1f)
        {
            Debug.Log($"[Input] {gameObject.name} - MoveX: {MoveX}, MoveY: {MoveY}");
        }
        JumpHeld = jump && jump.action.IsPressed();
        JumpPressed = jump && jump.action.WasPressedThisFrame();
        bool interactEnabled = interact && interact.action != null;
        InteractPressed = interactEnabled && interact.action.WasPressedThisFrame();
        InteractHeld = interactEnabled && interact.action.IsPressed();
        InteractReleased = interactEnabled && interact.action.WasReleasedThisFrame();

        FlashlightTogglePressed = flashlightToggle && flashlightToggle.action.WasPressedThisFrame();
        SwitchCharacterPressed = switchCharacter && switchCharacter.action.WasPressedThisFrame();
        MergeTogglePressed = mergeToggle && mergeToggle.action.WasPressedThisFrame();
    }

    public void ClearFrameEdges()
    {
        JumpPressed = false;
        InteractPressed = false;
        InteractReleased = false;
        FlashlightTogglePressed = false;
    }
}
