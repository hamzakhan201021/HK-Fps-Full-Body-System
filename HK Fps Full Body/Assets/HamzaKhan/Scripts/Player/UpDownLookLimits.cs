using UnityEngine;
using System;

[Serializable]
public class UpDownLookLimits
{
    [Range(0f, 89f)] public float LookUpLimit = 89f;
    [Range(0f, 89f)] public float LookDownLimit = 89f;
}
