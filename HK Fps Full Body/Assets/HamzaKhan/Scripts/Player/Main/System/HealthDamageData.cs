using UnityEngine;

namespace HKFps
{
    [System.Serializable]
    public class HealthDamageData
    {
        [Header("Shot Multipliers")]
        [Range(0, 1)] public float HeadShotMultiplier = 1;
        [Range(0, 1)] public float BodyShotMultiplier = 1;
        [Range(0, 1)] public float ArmShotMultiplier = 1;
        [Range(0, 1)] public float LegShotMultiplier = 1;
    }
}