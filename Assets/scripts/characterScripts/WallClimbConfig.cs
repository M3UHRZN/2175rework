using UnityEngine;

[CreateAssetMenu(menuName = "Configs/WallClimbConfig")]
public class WallClimbConfig : ScriptableObject
{
    [Header("Climbing")]
    [Min(0f)] public float climbSpeed = 3.5f;    // dikey hız
    [Range(0f, 1f)] public float stickFriction = 0.15f; // yatay kaymayı bastırma
    public float enterMinMoveY = 0.1f;           // tetiğe girmek için min Up/Down
    
    [Header("Exit Conditions")]
    public bool exitOnHorizontalMove = true;     // A/D ile çıkış
    public float horizontalMoveThreshold = 0.3f; // Yatay hareket eşiği
    public bool exitOnJump = true;               // Zıplama ile çıkış
    
    [Header("Jump Exit")]
    [Range(10f, 85f)] public float jumpExitAngle = 45f;    // Zıplama açısı
    [Min(0.1f)] public float jumpExitImpulse = 8f;         // Zıplama gücü
    public bool allowVariableJumpExit = true;              // Variable jump height
    [Range(0.1f, 1f)] public float variableJumpMultiplier = 0.8f;
}
