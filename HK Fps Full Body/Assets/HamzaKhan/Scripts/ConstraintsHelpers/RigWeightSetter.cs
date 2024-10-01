using UnityEngine.Animations.Rigging;
using UnityEngine;

public class RigWeightSetter : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private Rig rig;
    [SerializeField, Range(0, 1)] private float weight = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        rig.weight = weight;
    }
}
