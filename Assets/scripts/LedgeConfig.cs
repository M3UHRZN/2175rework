using UnityEngine;

[CreateAssetMenu(menuName = "Configs/LedgeConfig")]
public class LedgeConfig : ScriptableObject
{
    public float probeRadius = 0.12f;
    public float snapLerp = 0.85f;
    public Vector2 defaultClimbShift = new Vector2(0.5f, 0.9f);
    public float hangTimeoutMs = 0f; // 0 = unlimited
}
