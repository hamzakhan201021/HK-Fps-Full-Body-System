using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Volume))]
public class GlobalVolumeInstance : MonoBehaviour
{

    // Instance.
    public static GlobalVolumeInstance Instance;

    private void Awake()
    {
        // Set instance to this.
        Instance = this;
    }

    /// <summary>
    /// When using this, make sure to cache reference, because it uses Get Component
    /// </summary>
    /// <returns></returns>
    public Volume GetGlobalVolumeRef()
    {
        return GetComponent<Volume>();
    }
}
