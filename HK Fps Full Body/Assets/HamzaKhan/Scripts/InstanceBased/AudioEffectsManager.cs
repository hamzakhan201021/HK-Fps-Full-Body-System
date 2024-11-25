using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace HKFps
{
    public static class AudioEffectsManager
    {
        public static void PlayAudioEffect(AudioEffectSettings audioEffectSettings, Vector3 point, Transform parent = null)
        {
            GameObject audioEffect = new GameObject("Play Audio Effect");

            audioEffect.transform.position = point;
            audioEffect.transform.SetParent(parent);

            AudioSource audioSource = audioEffect.AddComponent<AudioSource>();

            audioSource.clip = GetAudioClipFromClips(audioEffectSettings.Clips);
            audioSource.volume = audioEffectSettings.Volume;
            audioSource.pitch = audioEffectSettings.Pitch;
            audioSource.panStereo = audioEffectSettings.StereoSpan;
            audioSource.spatialBlend = audioEffectSettings.SpatialBlend;
            audioSource.reverbZoneMix = audioEffectSettings.ReverbZoneMix;
            audioSource.dopplerLevel = audioEffectSettings.DopplerLevel;
            audioSource.spread = audioEffectSettings.Spread;
            audioSource.rolloffMode = audioEffectSettings.VolumeRollofMode;

            if (audioSource.rolloffMode == AudioRolloffMode.Custom)
            {
                try
                {
                    audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, audioEffectSettings.CustomRollofCurve);
                }
                catch
                {
                    // Failed to set custom curve, make sure to set the curve properly dumbo.
                }
            }

            audioSource.minDistance = audioEffectSettings.MinDistance;
            audioSource.maxDistance = audioEffectSettings.MaxDistance;

            audioSource.Play();

            Object.Destroy(audioEffect, audioSource.clip.length);
        }

        /// <summary>
        /// Start this coroutine to perform a delayed play.
        /// </summary>
        /// <param name="audioEffectSettings"></param>
        /// <param name="point"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static IEnumerator PlayAudioEffectWithDelay(AudioEffectSettings audioEffectSettings, Vector3 point, Transform parent = null)
        {
            if (audioEffectSettings.PlayDelay > 0.05f)
            {
                yield return new WaitForSeconds(audioEffectSettings.PlayDelay);
            }

            PlayAudioEffect(audioEffectSettings, point, parent);
        }

        private static AudioClip GetAudioClipFromClips(List<AudioClip> clips)
        {
            int index = Random.Range(0, clips.Count);

            return clips[index];
        }
    }
}