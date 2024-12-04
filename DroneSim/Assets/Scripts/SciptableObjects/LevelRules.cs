using UnityEngine;
[CreateAssetMenu(fileName = "LevelRules", menuName = "Create LevelRules", order = 1)]
public class LevelRules : ScriptableObject
{
    public float additionalGravity = 1.5f;
    public float groundEffectMultiplier = 1.325f;
    public int worldSize=300;
    public float globalSpeedModifier =1;
    public Vector3 spawn;//Gets set from spawner
    public Quaternion spawnRotation=Quaternion.identity;
}
