using UnityEngine;
[CreateAssetMenu(fileName = "DroneStats", menuName = "Create DroneStats", order = 1)]
public class DroneStats : ScriptableObject
{
    public float fakeGravity = 800;
    public float mass = 800;
    public float drag = 0.6f;
    public float angularDrag = 15;
    public float throttleModifier = 50;
    public float maxSpeed = 15f;
    public float yawSpeedModifier = 0.8f;
    public float pitchRollModifier = 0.5f;
    public float fieldOfView = 130;
}
