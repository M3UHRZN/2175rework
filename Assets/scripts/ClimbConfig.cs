using UnityEngine;

[CreateAssetMenu(menuName = "Configs/ClimbConfig")]
public class ClimbConfig : ScriptableObject
{
    [Min(0f)] public float climbSpeed = 4f;
    public float enterTolerance = 0.1f;
    public float exitThreshold  = 0.15f;
}
