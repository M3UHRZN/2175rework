using UnityEngine;

[CreateAssetMenu(menuName = "Configs/WallClimbConfig")]
public class WallClimbConfig : ScriptableObject
{
    [Min(0f)] public float climbSpeed = 3.5f;    // dikey hız
    [Range(0f, 1f)] public float stickFriction = 0.15f; // yatay kaymayı bastırma
    public float enterMinMoveY = 0.1f;           // tetiğe girmek için min Up/Down
}
