using UnityEngine;

[CreateAssetMenu(menuName = "Configs/WallConfig")]
public class WallConfig : ScriptableObject
{
    [Min(0.01f)] public float wallCheckDist = 0.2f;
    [Range(10f, 85f)] public float wallJumpAngle = 55f;
    [Min(0.1f)] public float wallJumpImpulse = 10f;
}
