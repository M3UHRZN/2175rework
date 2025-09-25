using UnityEngine;

[CreateAssetMenu(menuName = "Configs/WallConfig")]
public class WallConfig : ScriptableObject
{
    [Header("Wall Detection")]
    [Min(0.01f)] public float wallCheckDist = 0.2f;
    
    [Header("Wall Slide")]
    [Min(0f)] public float slideFriction = 0.1f; // Duvar kayma sürtünmesi
    [Min(0f)] public float slideAcceleration = 2f; // Kayma hızlanması
}
