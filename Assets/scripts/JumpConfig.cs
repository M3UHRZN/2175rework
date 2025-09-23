using UnityEngine;

[CreateAssetMenu(menuName = "Configs/JumpConfig")]
public class JumpConfig : ScriptableObject
{
    [Header("Heights (Unity units)")]
    public float shortJumpHeight = 1.25f;
    public float fullJumpHeight  = 2.6f;

    [Header("Timing (ms)")]
    public float jumpHoldMs = 180f; // variable jump window
    public float coyoteMs   = 150f; // after leaving ground
    public float bufferMs   = 150f; // before landing

    [Header("Cut / Ceiling")]
    public bool ceilingCancel = true;
    [Range(0.1f, 1f)] public float cutMultiplier = 0.55f;
}
