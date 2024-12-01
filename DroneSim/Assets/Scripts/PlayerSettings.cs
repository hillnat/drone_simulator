using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
[CreateAssetMenu(fileName = "PlayerSettings", menuName = "Create PlayerSettings", order = 1)]
public class PlayerSettings : ScriptableObject
{
    public float cameraAngle = -25;
    public AnimationCurve throttleCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve pitchRollCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
    public AnimationCurve yawCurve = new(new Keyframe(0, 0), new Keyframe(1, 1));
}
