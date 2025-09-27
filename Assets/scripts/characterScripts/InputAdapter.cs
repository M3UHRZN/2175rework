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

    [Header("Action Asset Fallback")]
    public InputActionAsset actionAsset;
    public string moveActionPath = "Player/Move";
    public string jumpActionPath = "Player/Jump";
    public string interactActionPath = "Player/Interact";
    public string flashlightToggleActionPath = "Player/FlashlightToggle";
    public string switchCharacterActionPath = "Player/SwitchCharacter";
    public string mergeToggleActionPath = "Player/MergeToggle";

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

    InputAction resolvedMoveAction;
    InputAction resolvedJumpAction;
    InputAction resolvedInteractAction;
    InputAction resolvedFlashlightToggleAction;
    InputAction resolvedSwitchCharacterAction;
    InputAction resolvedMergeToggleAction;

    void OnEnable()
    {
        ResolveAction(move, ref resolvedMoveAction, moveActionPath);
        ResolveAction(jump, ref resolvedJumpAction, jumpActionPath);
        ResolveAction(interact, ref resolvedInteractAction, interactActionPath);
        ResolveAction(flashlightToggle, ref resolvedFlashlightToggleAction, flashlightToggleActionPath);
        ResolveAction(switchCharacter, ref resolvedSwitchCharacterAction, switchCharacterActionPath);
        ResolveAction(mergeToggle, ref resolvedMergeToggleAction, mergeToggleActionPath);
    }
    void OnDisable()
    {
        DisableAction(ref resolvedMoveAction);
        DisableAction(ref resolvedJumpAction);
        DisableAction(ref resolvedInteractAction);
        DisableAction(ref resolvedFlashlightToggleAction);
        DisableAction(ref resolvedSwitchCharacterAction);
        DisableAction(ref resolvedMergeToggleAction);
    }

    void ResolveAction(InputActionReference reference, ref InputAction cache, string actionPath)
    {
        cache = reference ? reference.action : null;
        if (!cache && actionAsset && !string.IsNullOrEmpty(actionPath))
            cache = actionAsset.FindAction(actionPath, false);

        if (cache != null && !cache.enabled)
            cache.Enable();
    }

    static void DisableAction(ref InputAction action)
    {
        if (action != null)
        {
            if (action.enabled)
                action.Disable();
            action = null;
        }
    }

    public void Collect()
    {
        if (!InputEnabled)
        {
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

        if (resolvedMoveAction == null && actionAsset && move == null && !string.IsNullOrEmpty(moveActionPath))
            ResolveAction(null, ref resolvedMoveAction, moveActionPath);

        Vector2 mv = resolvedMoveAction != null ? resolvedMoveAction.ReadValue<Vector2>() : Vector2.zero;
        MoveX = Mathf.Clamp(mv.x, -1f, 1f);
        MoveY = Mathf.Clamp(mv.y, -1f, 1f);
        JumpHeld = resolvedJumpAction != null && resolvedJumpAction.IsPressed();
        JumpPressed = resolvedJumpAction != null && resolvedJumpAction.WasPressedThisFrame();
        bool interactEnabled = resolvedInteractAction != null;
        InteractPressed = interactEnabled && resolvedInteractAction.WasPressedThisFrame();
        InteractHeld = interactEnabled && resolvedInteractAction.IsPressed();
        InteractReleased = interactEnabled && resolvedInteractAction.WasReleasedThisFrame();

        FlashlightTogglePressed = resolvedFlashlightToggleAction != null && resolvedFlashlightToggleAction.WasPressedThisFrame();
        SwitchCharacterPressed = resolvedSwitchCharacterAction != null && resolvedSwitchCharacterAction.WasPressedThisFrame();
        MergeTogglePressed = resolvedMergeToggleAction != null && resolvedMergeToggleAction.WasPressedThisFrame();
    }

    public void ClearFrameEdges()
    {
        JumpPressed = false;
        InteractPressed = false;
        InteractReleased = false;
        FlashlightTogglePressed = false;
    }
}
