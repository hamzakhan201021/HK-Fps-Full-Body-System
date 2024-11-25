using UnityEngine;
using Unity.Cinemachine;

namespace HKFps
{
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
            impulseListener.ReactionSettings.AmplitudeGain = 1;
            impulseListener.ReactionSettings.FrequencyGain = 1;
            impulseListener.ReactionSettings.Duration = 1;

            impulseSource.GenerateImpulseWithForce(globalShakeForce);
        }

        public void CameraShakeVelocity(CinemachineImpulseSource impulseSource, Vector3 velocity)
        {
            impulseListener.ReactionSettings.AmplitudeGain = 1;
            impulseListener.ReactionSettings.FrequencyGain = 1;
            impulseListener.ReactionSettings.Duration = 1;

            impulseSource.GenerateImpulse(velocity);
        }

        public void CameraShakeFromProfile(CameraShakeProfile profile, CinemachineImpulseSource impulseSource)
        {
            SetupCameraShakeSettings(profile, impulseSource);

            impulseSource.GenerateImpulseWithForce(profile.ImpulseForce);
        }

        private void SetupCameraShakeSettings(CameraShakeProfile profile, CinemachineImpulseSource impulseSource)
        {
            impulseDefinition = impulseSource.ImpulseDefinition;

            impulseDefinition.ImpulseDuration = profile.ImpulseTime;
            impulseSource.DefaultVelocity = profile.DefaultVelocity;
            impulseDefinition.CustomImpulseShape = profile.impulseCurve;

            impulseListener.ReactionSettings.AmplitudeGain = profile.ListenerAmplitude;
            impulseListener.ReactionSettings.FrequencyGain = profile.ListenerFrequency;
            impulseListener.ReactionSettings.Duration = profile.ListenerDuration;
        }
    }
}