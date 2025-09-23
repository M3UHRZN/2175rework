using UnityEngine;

[CreateAssetMenu(menuName = "Configs/MovementConfig")]
public class MovementConfig : ScriptableObject
{
    [Header("Run / Air Control")]
    [Min(0.1f)] public float maxRunSpeed = 6f;
    [Tooltip("Seconds to reach target speed on ground")]
    [Min(0.01f)] public float accelTime = 0.12f;
    [Tooltip("Seconds to stop on ground")]
    [Min(0.01f)] public float decelTime = 0.12f;
    [Range(0f, 1f)] public float airControl = 0.55f;

    [Header("Gravity")]
    public float gravityScale = 3.5f;

    [Header("Wall Slide")]
    [Min(0f)] public float wallSlideMaxFall = 2.5f;
}
