using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeManager : MonoBehaviour
{
    public static CameraShakeManager instance;

    [SerializeField] private float globalShakeForce = 1f;
    [SerializeField] private CinemachineImpulseListener impulseListener;

    private CinemachineImpulseDefinition impulseDefinition;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    public void CameraShakeForce(CinemachineImpulseSource impulseSource)
    {
        impulseListener.m_ReactionSettings.m_AmplitudeGain = 1;
        impulseListener.m_ReactionSettings.m_FrequencyGain = 1;
        impulseListener.m_ReactionSettings.m_Duration = 1;

        impulseSource.GenerateImpulseWithForce(globalShakeForce);
    }

    public void CameraShakeVelocity(CinemachineImpulseSource impulseSource, Vector3 velocity)
    {
        impulseListener.m_ReactionSettings.m_AmplitudeGain = 1;
        impulseListener.m_ReactionSettings.m_FrequencyGain = 1;
        impulseListener.m_ReactionSettings.m_Duration = 1;

        impulseSource.GenerateImpulse(velocity);
    }

    public void CameraShakeFromProfile(CameraShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        SetupCameraShakeSettings(profile, impulseSource);

        impulseSource.GenerateImpulseWithForce(profile.ImpulseForce);
    }

    private void SetupCameraShakeSettings(CameraShakeProfile profile, CinemachineImpulseSource impulseSource)
    {
        impulseDefinition = impulseSource.m_ImpulseDefinition;

        impulseDefinition.m_ImpulseDuration = profile.ImpulseTime;
        impulseSource.m_DefaultVelocity = profile.DefaultVelocity;
        impulseDefinition.m_CustomImpulseShape = profile.impulseCurve;

        impulseListener.m_ReactionSettings.m_AmplitudeGain = profile.ListenerAmplitude;
        impulseListener.m_ReactionSettings.m_FrequencyGain = profile.ListenerFrequency;
        impulseListener.m_ReactionSettings.m_Duration = profile.ListenerDuration;
    }
}
