using UnityEngine;
using System;

namespace HKFps
{
    [Serializable]
    public class HandsIKTransform
    {

        [Header("Left Hand IK Transforms")]
        public Transform LeftHandIKTransform;

        [Space(5)]
        public Transform LeftHandIndexIKTransform;
        public Transform LeftHandMiddleIKTransform;
        public Transform LeftHandPinkyIKTransform;
        public Transform LeftHandRingIKTransform;
        public Transform LeftHandThumbIKTransform;

        [Header("Right Hand IK Transforms")]
        public Transform RightHandIKTransform;

        [Space(5)]
        public Transform RightHandIndexIKTransform;
        public Transform RightHandMiddleIKTransform;
        public Transform RightHandPinkyIKTransform;
        public Transform RightHandRingIKTransform;
        public Transform RightHandThumbIKTransform;

    }
}