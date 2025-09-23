using UnityEngine;

[DisallowMultipleComponent]
public class LedgeMarker : MonoBehaviour
{
    [Tooltip("Hang pivot world offset relative to this trigger center")]
    public Vector2 hangOffset = new Vector2(0f, -0.05f);
    [Tooltip("Climb up shift (local), x yönü kenar yönüne göre ± uygulanır")]
    public Vector2 climbUpShift = new Vector2(0.5f, 0.9f);
    [Tooltip("Which side player should face when hanging: -1 left, +1 right, 0 auto")]
    public int preferFacing = 0;

    public Vector2 WorldHangPoint => (Vector2)transform.position + hangOffset;
}
