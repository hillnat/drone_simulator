using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Create PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public enum AltitudeUnits { meters, kilometers, feet }

    public enum SpeedUnits { metersPerSecond, kilometersPerHour, feetPerSecond, milesPerHour }
    public float cameraAngle = -25;
    public AnimationCurve throttleCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve pitchRollCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve yawCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public float angleModeMaxAngle = 75f;
    public float masterVolume = 1f;
    public float soundFxVolume = 1f;
    public Vector3 eyeSize = Vector3.one;
    public Vector3 eyePosition = Vector3.zero;
    
    public OSD_Element OSD_speed = new OSD_Element {};
    public OSD_Element OSD_altitude = new OSD_Element {};
    public OSD_Element OSD_horizonLine = new OSD_Element {};
    public OSD_Element OSD_Crosshair = new OSD_Element {};
   
}
[Serializable]
public struct OSD_Element
{
    public GameObject uiElement;
    public Vector2 position;
    public Vector2 scale;
}
