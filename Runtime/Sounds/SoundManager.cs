using System.Collections.Generic;
using UnityEngine;

namespace SinbadStudios.SharedSystems.Runtime
{
    [System.Serializable]
    public class NamedClip
    {
        public string key;
        public AudioClip clip;
    }

    public class SoundManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource[] musicSources;

        [Header("Clips")]
        [SerializeField] private List<NamedClip> clips = new List<NamedClip>();

        private Dictionary<string, AudioClip> _map;

        private void Awake()
        {
            GameEventBus.Instance.Subscribe<PlaySoundEffectsEvent>(OnPlaySoundEffect);
            GameEventBus.Instance.Subscribe<PlayMusicEvent>(OnPlayMusic);
            GameEventBus.Instance.Subscribe<StopMusicEvent>(DoStopMusic);

            _map = new Dictionary<string, AudioClip>();
            foreach (var nc in clips)
            {
                if (nc != null && !string.IsNullOrEmpty(nc.key) && nc.clip != null)
                {
                    _map[nc.key] = nc.clip;
                }
            }
        }

        private void OnPlaySoundEffect(PlaySoundEffectsEvent eventData)
        {
            if (sfxSource == null)
            {
                return;
            }
            if (_map != null && _map.TryGetValue(eventData.Key, out var clip))
            {
                sfxSource.PlayOneShot(clip, eventData.Volume);
            }
        }

        private void OnPlayMusic(PlayMusicEvent eventData)
        {
            if (musicSources == null || musicSources.Length == 0)
            {
                return;
            }

            int channel = Mathf.Clamp(eventData.Channel, 0, musicSources.Length - 1);
            AudioSource targetSource = musicSources[channel];

            if (targetSource == null)
            {
                return;
            }

            if (_map != null && _map.TryGetValue(eventData.Key, out var clip))
            {
                if (targetSource.clip != clip)
                {
                    targetSource.clip = clip;
                }
                targetSource.loop = eventData.Loop;
                targetSource.volume = eventData.Volume;
                targetSource.Play();
            }
        }

        private void DoStopMusic(StopMusicEvent eventData)
        {
            if (musicSources == null || musicSources.Length == 0)
            {
                return;
            }

            if (eventData.Channel >= 0 && eventData.Channel < musicSources.Length)
            {
                musicSources[eventData.Channel]?.Stop();
            }
            else
            {
                foreach (var source in musicSources)
                {
                    source?.Stop();
                }
            }
        }

        private void OnDestroy()
        {
            GameEventBus.Instance.Unsubscribe<PlaySoundEffectsEvent>(OnPlaySoundEffect);
            GameEventBus.Instance.Unsubscribe<PlayMusicEvent>(OnPlayMusic);
            GameEventBus.Instance.Unsubscribe<StopMusicEvent>(DoStopMusic);
        }
    }
}
