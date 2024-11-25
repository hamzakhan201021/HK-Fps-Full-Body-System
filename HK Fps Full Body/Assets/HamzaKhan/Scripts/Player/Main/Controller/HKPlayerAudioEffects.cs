using System.Collections.Generic;
using UnityEngine;

namespace HKFps
{
    public class HKPlayerAudioEffects : MonoBehaviour
    {

        #region Fields

        [Space]
        [Header("Audio Effects")]
        [SerializeField] private Transform _audioEffectsParent;
        [Space]
        [SerializeField] private AudioEffectSettings _footAudioEffect;
        [SerializeField] private AudioEffectSettings _playerDamageAudioEffect;
        [SerializeField] private AudioEffectSettings _playerDeathAudioEffect;
        [SerializeField] private AudioEffectSettings _playerSwitchWeaponAudioEffect;
        [SerializeField] private AudioEffectSettings _playerThrowObjAudioEffect;
        [SerializeField] private AudioEffectSettings _playerJumpAudioEffect;
        [SerializeField] private AudioEffectSettings _playerLandAudioEffect;
        [Space]
        [Header("Audio Sources")]
        [SerializeField] private float _breathingAudioLerpSpeed = 1;
        [Space]
        [SerializeField] private AudioSourceSettings _walkingBreathingAudio;
        [SerializeField] private AudioSourceSettings _sprintingBreathingAudio;

        #endregion

        #region General

        private void Start()
        {
            Setup();
        }

        private void Setup()
        {
            _sprintingBreathingAudio.AudioSource.Play();
            _walkingBreathingAudio.AudioSource.Play();

            _sprintingBreathingAudio.AudioSource.volume = _sprintingBreathingAudio.MinVolume;
            _walkingBreathingAudio.AudioSource.volume = _walkingBreathingAudio.MaxVolume;
        }

        public void UpdateAudioSystem(bool sprinting)
        {
            HandleWalkSprintAudioSources(sprinting);
        }

        private void HandleWalkSprintAudioSources(bool sprinting)
        {
            float dBreathVolume = Mathf.Lerp(_walkingBreathingAudio.AudioSource.volume, sprinting ?
                _walkingBreathingAudio.MinVolume : _walkingBreathingAudio.MaxVolume, _breathingAudioLerpSpeed * Time.deltaTime);

            float sprintingBreathVolume = Mathf.Lerp(_sprintingBreathingAudio.AudioSource.volume, sprinting ?
                _sprintingBreathingAudio.MaxVolume : _sprintingBreathingAudio.MinVolume, _breathingAudioLerpSpeed * Time.deltaTime);

            _walkingBreathingAudio.AudioSource.volume = dBreathVolume;
            _sprintingBreathingAudio.AudioSource.volume = sprintingBreathVolume;
        }

        #endregion

        #region Play Audio Effect Events

        public void OnPlayerFootLand()
        {
            PlayAudioEffect(_footAudioEffect);
        }

        public void OnPlayerDamage(float prevHealth, float newHealth)
        {
            PlayAudioEffect(_playerDamageAudioEffect);
        }

        public void OnPlayerDeath()
        {
            PlayAudioEffect(_playerDeathAudioEffect);
        }

        public void OnPlayerSwitchWeapon(WeaponBase weapon)
        {
            PlayAudioEffect(_playerSwitchWeaponAudioEffect);
        }

        public void OnPlayerThrowObj()
        {
            PlayAudioEffect(_playerThrowObjAudioEffect);
        }

        public void OnPlayerJump()
        {
            PlayAudioEffect(_playerJumpAudioEffect);
        }

        public void OnPlayerLand(float velocity)
        {
            PlayAudioEffect(_playerLandAudioEffect);
        }

        public void StopAllAudio()
        {
            _walkingBreathingAudio.AudioSource.Stop();
            _sprintingBreathingAudio.AudioSource.Stop();
        }

        #endregion

        #region Extracts

        private void PlayAudioEffect(AudioEffectSettings audioEffectSettings)
        {
            StartCoroutine(AudioEffectsManager.PlayAudioEffectWithDelay(audioEffectSettings, transform.position, _audioEffectsParent));
        }

        #endregion
    }

    [System.Serializable]
    public class AudioEffectSettings
    {
        [Header("Audio Play Settings")]
        public List<AudioClip> Clips;
        public float PlayDelay = 0;
        public float Volume = 1;
        public float Pitch = 1;
        public float StereoSpan = 0;
        public float SpatialBlend = 1;
        public float ReverbZoneMix = 1;
        [Space]
        [Header("3D Sound Settings")]
        public float DopplerLevel = 1;
        public float Spread = 0;
        public AudioRolloffMode VolumeRollofMode = AudioRolloffMode.Logarithmic;
        public AnimationCurve CustomRollofCurve;
        public float MinDistance = 1;
        public float MaxDistance = 500;
    }

    [System.Serializable]
    public class AudioSourceSettings
    {
        [Header("Audio Source Playing Settings")]
        public AudioSource AudioSource;
        [Range(0, 1)] public float MinVolume = 0;
        [Range(0, 1)] public float MaxVolume = 1;
    }
}