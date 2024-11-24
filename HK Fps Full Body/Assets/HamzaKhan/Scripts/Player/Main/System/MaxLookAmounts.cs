using UnityEngine;
using System;

[Serializable]
public class MaxLookAmounts
{
    [Space(3)]
    public UpDownLookLimits LookAmountNormal;
    [Space(3)]
    public UpDownLookLimits LookAmountUnGround;
    [Space(3)]
    public UpDownLookLimits LookAmountProned;
    [Space(3)]
    public UpDownLookLimits LookAmountMoving;
}