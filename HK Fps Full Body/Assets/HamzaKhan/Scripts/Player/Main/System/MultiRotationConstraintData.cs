using UnityEngine.Animations.Rigging;
using UnityEngine;

[System.Serializable]
public class MultiRotationConstraintData
{
    public MultiRotationConstraint MultiRotationConstraint;
    [Tooltip("Optional Hierarchy Bone, Used for fakely getting the predicted rot/pos of the spine when it isn't applied")]
    public Transform HRotationBone;
    [Range(0, 1)] public float WeightScale = 1;
    public bool ApplyWeaponRotationOffsetY = true;
}
