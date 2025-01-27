using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Create PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public enum AltitudeUnits { meters, kilometers, feet }

    public enum SpeedUnits { metersPerSecond, kilometersPerHour, feetPerSecond, milesPerHour }
    public float cameraAngle = -25;
    public AnimationCurve throttleCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve pitchCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve rollCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve yawCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public float angleModeMaxAngle = 75f;
    public float masterVolume = 1f;
    public float soundFxVolume = 1f;
    public Vector3 eyeSize = Vector3.one;
    public Vector3 eyePosition = Vector3.zero;

    public OsdElementData[] allOsdElemDatas = new OsdElementData[7]{ 
        new OsdElementData("speed", true, new Vector2(500f, -400f), new Vector2(1f, 1f)),
        new OsdElementData("altitude", true, new Vector2(500f, -430f), new Vector2(1f, 1f)),
        new OsdElementData("horizonLine", false, new Vector2(0, 0), new Vector2(1f, 1f)),
        new OsdElementData("crosshair", true, new Vector2(0, 0), new Vector2(1f, 1f)),
        new OsdElementData("timer", true, new Vector2(-400, 0), new Vector2(1f, 1f)),
        new OsdElementData("name", true, new Vector2(0, -500), new Vector2(1f, 1f)),
        new OsdElementData("fps", true, new Vector2(1000, 900), new Vector2(1f, 1f))
    };
}
[Serializable]
public struct OsdElementData
{
    public string elementName;
    public bool elementEnabled;
    public Vector2 position;
    public Vector2 scale;
    public OsdElementData(string name, bool enabled, Vector2 position, Vector2 scale)
    {
        this.elementName = name;
        this.elementEnabled = enabled;
        this.position = position;
        this.scale = scale;
    }
}
