using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSyringeItem : ItemBase
{
    public int HealthAddition = 40;

    public override void StartUse(HKPlayerItemSystem controller)
    {
        
    }
   
    public override void HoldUse(HKPlayerItemSystem controller)
    {
        
    }

    public override void ReleaseUse(HKPlayerItemSystem controller)
    {
        controller.AddHealth(HealthAddition);
        controller.OnUseComplete();
    }

    public override void CancelUse(HKPlayerItemSystem controller)
    {
        
    }
}
