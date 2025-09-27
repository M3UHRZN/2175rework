using UnityEngine;

[CreateAssetMenu(menuName = "Camera/Party Profile", fileName = "CameraProfile")]
public class CameraProfile : ScriptableObject
{
    public Vector3 followOffset = new Vector3(0.6f, 1.6f, -8f);
    public float positionDamping = 0.15f;
    public float fov = 55f;
}
