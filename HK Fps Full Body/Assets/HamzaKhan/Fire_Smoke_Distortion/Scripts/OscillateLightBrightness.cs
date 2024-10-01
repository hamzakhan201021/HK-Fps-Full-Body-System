using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscillateLightBrightness : MonoBehaviour
{
    Light lightComponent;
    [SerializeField, Range(0f, 10f)]
    float lower;

    [SerializeField, Range(0f, 10f)]
    float upper;
    // Start is called before the first frame update
    void Start()
    {
        lightComponent = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        lightComponent.intensity = Random.Range(lower, upper);
    }
}
