using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// CHANGES NEW SCRIPT
public class AlphaHitModifier : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private Image _image;
    [SerializeField] private float alphaHitThreshold = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        _image.alphaHitTestMinimumThreshold = alphaHitThreshold;
    }
}
