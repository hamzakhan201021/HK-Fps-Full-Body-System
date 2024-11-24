using UnityEngine;

[CreateAssetMenu(fileName = "CameraShakeProfile", menuName = "ScriptableObjects/CameraShakeProfile")]
public class CameraShakeProfile : ScriptableObject
{
    [Header("Impulse Source Settings")]
    public float ImpulseTime = 0.2f;
    public float ImpulseForce = 1f;
    public Vector3 DefaultVelocity = new Vector3(0f, -1f, 0f);
    public AnimationCurve impulseCurve;

    [Header("Impulse Listener Settings")]
    public float ListenerAmplitude = 1;
    public float ListenerFrequency = 1;
    public float ListenerDuration = 1;
}