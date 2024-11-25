using UnityEngine;
using UnityEngine.Animations.Rigging;

using Unity.Burst;
using Unity.Mathematics;

namespace HKFps
{
    [BurstCompile]
    public struct MatchRotationConstraintWorldJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle constrained;
        public ReadOnlyTransformHandle toMatch;

        public FloatProperty jobWeight { get; set; }

        public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream) { }

        public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
        {
            float w = jobWeight.Get(stream);
            if (w > 0f)
            {
                constrained.SetRotation(
                    stream,
                    math.slerp(constrained.GetRotation(stream), toMatch.GetRotation(stream), w)
                    );
            }
        }
    }

    [System.Serializable]
    public struct MatchRotationConstraintData : IAnimationJobData
    {
        public Transform constrainedObject;
        [SyncSceneToStream] public Transform toMatchObject;

        public bool IsValid()
        {
            return !(constrainedObject == null || toMatchObject == null);
        }

        public void SetDefaultValues()
        {
            constrainedObject = null;
            toMatchObject = null;
        }
    }

    public class MatchRotationBinder : AnimationJobBinder<MatchRotationConstraintWorldJob, MatchRotationConstraintData>
    {
        public override MatchRotationConstraintWorldJob Create(Animator animator, ref MatchRotationConstraintData data, Component component)
        {
            return new MatchRotationConstraintWorldJob()
            {
                constrained = ReadWriteTransformHandle.Bind(animator, data.constrainedObject),
                toMatch = ReadOnlyTransformHandle.Bind(animator, data.toMatchObject)
            };
        }

        public override void Destroy(MatchRotationConstraintWorldJob job) { }
    }

    [DisallowMultipleComponent, AddComponentMenu("Custom Animation Rigging/Match Rotation Constraint")]
    public class MatchRotationConstraint : RigConstraint<MatchRotationConstraintWorldJob, MatchRotationConstraintData, MatchRotationBinder> { }
}