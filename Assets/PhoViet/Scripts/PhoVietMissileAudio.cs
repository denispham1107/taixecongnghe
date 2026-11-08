using System.Collections.Generic;
using UnityEngine;

namespace PhoViet
{
    public partial class PhoVietGame
    {
        readonly AudioSource[] missileLaunchVoices = new AudioSource[8];
        readonly List<AudioSource> missileAudioSources = new List<AudioSource>(60);
        AudioClip missileLaunchClip, missileFlightClip;
        int nextMissileLaunchVoice;
        bool missileAudioMuted;

        void InitializeMissileAudio()
        {
            missileLaunchClip = Resources.Load<AudioClip>("Audio/PV_RocketLaunch");
            missileFlightClip = Resources.Load<AudioClip>("Audio/PV_RocketFlight");
            missileAudioMuted = !sound;
            for (int i = 0; i < missileLaunchVoices.Length; i++)
            {
                var emitter = new GameObject("Rocket launch audio");
                emitter.transform.SetParent(transform, false);
                missileLaunchVoices[i] = CreateMissileAudioSource(emitter.transform, missileLaunchClip, false);
            }
        }

        AudioSource CreateMissileFlightAudio(Transform missile, bool player)
        {
            var source = CreateMissileAudioSource(missile, missileFlightClip, true);
            source.volume = player ? .56f : .42f;
            source.minDistance = player ? 14 : 8;
            source.maxDistance = 105;
            source.pitch = player ? 1.06f : .92f;
            return source;
        }

        AudioSource CreateMissileAudioSource(Transform emitter, AudioClip clip, bool loop)
        {
            var source = PhoVietAudio.Route(emitter.gameObject.AddComponent<AudioSource>());
            source.clip = clip;
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 1;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.dopplerLevel = 0;
            source.mute = missileAudioMuted;
            missileAudioSources.Add(source);
            return source;
        }

        void PlayMissileAudio(AudioSource flight, Vector3 origin, bool player)
        {
            if (flight != null && flight.clip != null) flight.Play();
            if (missileLaunchClip == null) return;
            // A launch survives even if the projectile hits something in its first step.
            var launch = missileLaunchVoices[nextMissileLaunchVoice];
            nextMissileLaunchVoice = (nextMissileLaunchVoice + 1) % missileLaunchVoices.Length;
            launch.transform.localPosition = origin;
            launch.spatialBlend = player ? .4f : 1;
            launch.minDistance = 12;
            launch.maxDistance = 140;
            launch.volume = player ? .78f : .56f;
            launch.pitch = player ? 1 : .9f;
            launch.Play();
        }

        void UpdateMissileAudioMute()
        {
            bool mute = !sound;
            if (missileAudioMuted == mute) return;
            missileAudioMuted = mute;
            foreach (var source in missileAudioSources) source.mute = mute;
        }

        void PauseMissileAudio(bool paused)
        {
            foreach (var source in missileAudioSources)
            {
                if (!source.gameObject.activeInHierarchy) continue;
                if (paused) source.Pause();
                else source.UnPause();
            }
        }

        void ResetMissileLaunchAudio()
        {
            foreach (var source in missileLaunchVoices) if (source != null) source.Stop();
            nextMissileLaunchVoice = 0;
        }

        void ScrollMissileLaunchAudio(float distance)
        {
            foreach (var source in missileLaunchVoices)
                if (source != null && source.isPlaying)
                    source.transform.localPosition -= Vector3.forward * distance;
        }

        void RebaseMissileLaunchAudio(Quaternion inverse, Vector3 origin)
        {
            foreach (var source in missileLaunchVoices)
                if (source != null && source.isPlaying)
                    source.transform.localPosition = inverse * (source.transform.localPosition - origin);
        }
    }
}
