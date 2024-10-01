using UnityEngine.Animations.Rigging;
using UnityEngine;

[System.Serializable]
public class MultiRotationConstraintData
{
    public MultiRotationConstraint MultiRotationConstraint;
    [Range(0, 1)] public float WeightScale = 1;
    public bool ApplyWeaponRotationOffsetY = true;
}
