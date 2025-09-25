using UnityEngine;

[CreateAssetMenu(menuName = "Configs/WallJumpConfig")]
public class WallJumpConfig : ScriptableObject
{
    [Header("Wall Jump Physics")]
    [Range(10f, 85f)] public float wallJumpAngle = 55f;
    [Min(0.1f)] public float wallJumpImpulse = 10f;
    
    [Header("Timing")]
    [Min(0f)] public float inputBufferMs = 100f; // Wall jump input buffer
    [Min(0f)] public float cooldownMs = 200f;    // Wall jump cooldown
    
    [Header("Visual/Feel")]
    [Range(0f, 1f)] public float wallPushForce = 0.3f; // Duvarı itme hissi
    public bool allowVariableJump = true; // Variable jump height
    [Range(0.1f, 1f)] public float variableJumpMultiplier = 0.7f;
}
