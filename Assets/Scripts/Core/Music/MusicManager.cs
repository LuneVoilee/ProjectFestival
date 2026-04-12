#region

using System.Collections.Generic;
using Core.Reactive;
using Tool;
using UnityEngine;

#endregion

namespace Core.Music
{
    [RequireComponent(typeof(MusicEventsHandler))]
    public class MusicManager : PersistentSingletonMono<MusicManager>
    {
        private AudioSource m_BgmSource;
        private AudioSource m_SfxSource;
        private Dictionary<string, AudioClip> m_AudioClips;

        public ReactiveValue<float> Volume = new(-1f);

        protected override void Awake()
        {
            base.Awake();

            InitMusic();

            GPEvents.GetVolume = () => Volume;
        }


        private void InitMusic()
        {
            m_BgmSource = gameObject.AddComponent<AudioSource>();
            // The BGM needs to loop
            m_BgmSource.loop = true;
            m_BgmSource.playOnAwake = false;

            m_SfxSource = gameObject.AddComponent<AudioSource>();
            m_SfxSource.playOnAwake = false;

            m_AudioClips = new Dictionary<string, AudioClip>();

            LoadAll();
        }

        private void OnEnable()
        {
            UIEvents.OnHopeVolumeChangeAction += HandleVolumeChange;
            UIEvents.OnAfterBindSettingsAction += InitVolume;
        }

        private void OnDisable()
        {
            UIEvents.OnHopeVolumeChangeAction -= HandleVolumeChange;
            UIEvents.OnAfterBindSettingsAction -= InitVolume;
        }

        private void HandleVolumeChange(float value)
        {
            Volume.Value = value;
            SetVolume(value);
        }

        private void SetVolume(float value)
        {
            m_BgmSource.volume = value;
            m_SfxSource.volume = value;
        }

        private void Start()
        {
            PlayBGM("BGM_1");
        }

        private void Update()
        {
        }

        private void InitVolume()
        {
            Volume.Value = 0.5f;
            SetVolume(0.5f);
        }

        public void PlayBGM(string musicName)
        {
            if (m_AudioClips.TryGetValue(musicName, out var clip))
            {
                if (m_BgmSource.clip != clip || !m_BgmSource.isPlaying)
                {
                    m_BgmSource.clip = clip;
                    m_BgmSource.Play();
                }
            }
            else
            {
                Debug.LogWarning($"Clip cannot find: {musicName}");
            }
        }


        public void StopBGM()
        {
            m_BgmSource.Stop();
        }


        public void PlaySFX(string musicName)
        {
            if (m_AudioClips.TryGetValue(musicName, out var clip))
            {
                m_SfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning($"Clip cannot find: {musicName}");
            }
        }

        private void LoadAll()
        {
            LoadFromFolder("Music/BGM");
            LoadFromFolder("Music/SFX");
        }

        private void LoadFromFolder(string folder)
        {
            var clips = Resources.LoadAll<AudioClip>(folder);
            foreach (var clip in clips)
            {
                m_AudioClips.TryAdd(clip.name, clip);
            }
        }
    }
}