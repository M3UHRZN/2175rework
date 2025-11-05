using System.Collections.Generic;
using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// Handles global audio volumes for the project.
    /// Maintains master, music and SFX multipliers and applies them to
    /// registered audio sources. Values are persisted using PlayerPrefs.
    /// </summary>
    public class AudioSettingsManager : MonoBehaviour
    {
        private const string MasterPrefKey = "Audio.Master";
        private const string MusicPrefKey = "Audio.Music";
        private const string SfxPrefKey = "Audio.Sfx";

        public static AudioSettingsManager Instance { get; private set; }

        [Range(0.0001f, 1f)]
        [SerializeField] private float defaultMasterVolume = 0.8f;
        [Range(0.0001f, 1f)]
        [SerializeField] private float defaultMusicVolume = 0.8f;
        [Range(0.0001f, 1f)]
        [SerializeField] private float defaultSfxVolume = 0.8f;

        private float masterVolume;
        private float musicVolume;
        private float sfxVolume;

        private readonly List<AudioSource> musicSources = new();
        private readonly List<AudioSource> sfxSources = new();

        public float MasterVolume => masterVolume;
        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
            ApplyVolumes();
        }

        private void LoadPreferences()
        {
            masterVolume = PlayerPrefs.GetFloat(MasterPrefKey, defaultMasterVolume);
            musicVolume = PlayerPrefs.GetFloat(MusicPrefKey, defaultMusicVolume);
            sfxVolume = PlayerPrefs.GetFloat(SfxPrefKey, defaultSfxVolume);
        }

        public void SetMasterVolume(float value)
        {
            masterVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MasterPrefKey, masterVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(MusicPrefKey, musicVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxPrefKey, sfxVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        private void ApplyVolumes()
        {
            AudioListener.volume = masterVolume;

            var musicMultiplier = masterVolume * musicVolume;
            var sfxMultiplier = masterVolume * sfxVolume;

            foreach (var source in musicSources)
            {
                if (source != null)
                {
                    source.volume = musicMultiplier;
                }
            }

            foreach (var source in sfxSources)
            {
                if (source != null)
                {
                    source.volume = sfxMultiplier;
                }
            }
        }

        public void RegisterMusicSource(AudioSource source)
        {
            if (source == null || musicSources.Contains(source))
            {
                return;
            }

            musicSources.Add(source);
            source.volume = masterVolume * musicVolume;
        }

        public void RegisterSfxSource(AudioSource source)
        {
            if (source == null || sfxSources.Contains(source))
            {
                return;
            }

            sfxSources.Add(source);
            source.volume = masterVolume * sfxVolume;
        }

        public void UnregisterSource(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            if (musicSources.Remove(source) || sfxSources.Remove(source))
            {
                source.volume = masterVolume;
            }
        }
    }
}
