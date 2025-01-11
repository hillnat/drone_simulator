using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public DroneStats droneStats;
    public Transform cameraMount;
    public Transform[] propellors = new Transform[4];
    public Vector3 propAxis = Vector3.forward;
}
