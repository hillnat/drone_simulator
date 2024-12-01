using UnityEngine;
[CreateAssetMenu(fileName = "DroneStats", menuName = "Create DroneStats", order = 1)]
public class DroneStats : ScriptableObject
{
    public float weight = 800;
    public float drag = 0.6f;
    public float angularDrag = 15;
    public float throttleModifier = 50;
    public float yawSpeedModifier = 0.8f;
    public float pitchRollModifier = 0.5f;
    public float fieldOfView = 130;
    public Vector3 cameraOffset = new Vector3(0, 0.01489f, 0.1026f);
    public Vector3 droneModelPositionOffset = Vector3.zero;
    public Vector3 droneModelRotationOffset = Vector3.zero;
    public Vector3 colliderCenter = Vector3.zero;
    public Vector3 colliderSize = Vector3.zero;
}
