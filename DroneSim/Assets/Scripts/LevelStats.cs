using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class GameManager
{
    public static float additionalGravity = 1.5f;
    public static float groundEffectMultiplier = 1.325f;
    public static int worldSize=300;
    public static float globalSpeedModifier =1;
    public static Vector3 spawn;//Gets set from spawner
    public static Quaternion spawnRotation=Quaternion.identity;
}
