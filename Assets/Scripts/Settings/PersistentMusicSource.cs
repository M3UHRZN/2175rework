using UnityEngine;

namespace Game.Settings
{
    /// <summary>
    /// Provides a persistent music source that survives scene loads
    /// and stays in sync with the <see cref="AudioSettingsManager"/>.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PersistentMusicSource : MonoBehaviour
    {
        [SerializeField] private AudioClip initialClip;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool loop = true;

        private AudioSource audioSource;
        private bool hasStarted;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = loop;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            TryRegister();

            if (playOnStart && !hasStarted)
            {
                StartPlayback();
            }
        }

        private void OnDisable()
        {
            var manager = AudioSettingsManager.Instance;
            if (manager != null && audioSource != null)
            {
                manager.UnregisterSource(audioSource);
            }
        }

        public void StartPlayback()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null || initialClip == null)
            {
                return;
            }

            audioSource.Stop();
            audioSource.clip = initialClip;
            audioSource.loop = loop;
            TryRegister();
            audioSource.Play();
            hasStarted = true;
        }

        public void StopPlayback()
        {
            if (audioSource == null)
            {
                return;
            }

            audioSource.Stop();
            hasStarted = false;
        }

        private void TryRegister()
        {
            var manager = AudioSettingsManager.Instance;
            if (manager != null && audioSource != null)
            {
                manager.RegisterMusicSource(audioSource);
            }
        }
    }
}
