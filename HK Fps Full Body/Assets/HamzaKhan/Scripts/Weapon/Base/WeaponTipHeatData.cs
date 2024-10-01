using UnityEngine;
using System;

[Serializable]
public class WeaponTipHeatData
{
    [Tooltip("The tip renderer")]
    public Renderer TipRenderer;
    [Tooltip("The Material that is assigned to you weapon's tip by default")]
    public Material TipMaterial;
    [Tooltip("The Emission Color parameter name of the weapon tip material")]
    public string EmissionColorParameterName = "_EmissionColor";
    [Tooltip("The Emission Color parameter name Param ID")]
    public int EmissionColorParameterNameID;
}
