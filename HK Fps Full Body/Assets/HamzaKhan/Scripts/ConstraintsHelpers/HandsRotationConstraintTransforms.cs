using UnityEngine;
using System;

namespace HKFps
{
    [Serializable]
    public class HandsRotationConstraintTransforms
    {
        [Header("Left Hand IK Transforms")]
        public Transform LeftHandIKTransform;

        [Space(10)]
        public Transform LeftHandIndex1ConstraintTransform;
        public Transform LeftHandIndex2ConstraintTransform;
        public Transform LeftHandIndex3ConstraintTransform;

        [Space(5)]
        public Transform LeftHandMiddle1ConstraintTransform;
        public Transform LeftHandMiddle2ConstraintTransform;
        public Transform LeftHandMiddle3ConstraintTransform;

        [Space(5)]
        public Transform LeftHandPinky1ConstraintTransform;
        public Transform LeftHandPinky2ConstraintTransform;
        public Transform LeftHandPinky3ConstraintTransform;

        [Space(5)]
        public Transform LeftHandRing1ConstraintTransform;
        public Transform LeftHandRing2ConstraintTransform;
        public Transform LeftHandRing3ConstraintTransform;

        [Space(5)]
        public Transform LeftHandThumb1ConstraintTransform;
        public Transform LeftHandThumb2ConstraintTransform;
        public Transform LeftHandThumb3ConstraintTransform;

        [Header("Right Hand IK Transforms")]
        public Transform RightHandIKTransform;

        [Space(10)]
        public Transform RightHandIndex1ConstraintTransform;
        public Transform RightHandIndex2ConstraintTransform;
        public Transform RightHandIndex3ConstraintTransform;

        [Space(5)]
        public Transform RightHandMiddle1ConstraintTransform;
        public Transform RightHandMiddle2ConstraintTransform;
        public Transform RightHandMiddle3ConstraintTransform;

        [Space(5)]
        public Transform RightHandPinky1ConstraintTransform;
        public Transform RightHandPinky2ConstraintTransform;
        public Transform RightHandPinky3ConstraintTransform;

        [Space(5)]
        public Transform RightHandRing1ConstraintTransform;
        public Transform RightHandRing2ConstraintTransform;
        public Transform RightHandRing3ConstraintTransform;

        [Space(5)]
        public Transform RightHandThumb1ConstraintTransform;
        public Transform RightHandThumb2ConstraintTransform;
        public Transform RightHandThumb3ConstraintTransform;
    }
}