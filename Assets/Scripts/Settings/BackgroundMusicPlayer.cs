using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Settings;

/// <summary>
/// Plays background music throughout the entire game.
/// Automatically persists across scene loads and registers with AudioSettingsManager.
/// Uses a list-based system to manage multiple music tracks.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusicPlayer : MonoBehaviour
{
    public static BackgroundMusicPlayer Instance { get; private set; }

    [System.Serializable]
    public class MusicTrack
    {
        [Tooltip("Sahne adı (örn: level1, menu, baslangicMenu)")]
        public string sceneName = "";
        
        [Tooltip("Bu sahne için çalınacak müzik")]
        public AudioClip clip;
        
        [Tooltip("Müzik loop yapılsın mı?")]
        public bool loop = true;
        
        [Tooltip("Müziğin kaçıncı saniyeden başlayacağı (0 = baştan)")]
        [Min(0f)]
        public float startTime = 0f;
    }

    [Header("Music Tracks")]
    [Tooltip("Sahne bazlı müzik listesi. Her sahne için özel müzik atayabilirsiniz.")]
    [SerializeField] private List<MusicTrack> musicTracks;
    [SerializeField] private AudioClip defaultMenuMusic;
    [SerializeField] private AudioClip defaultGameMusic;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool autoSwitchByScene = true;

    private AudioSource audioSource;
    private bool isRegistered;
    private string currentSceneName;
    private AudioClip currentPlayingClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        audioSource = GetComponent<AudioSource>();
        
        // Listeyi initialize et
        if (musicTracks == null)
        {
            musicTracks = new List<MusicTrack>();
        }
        
        ConfigureAudioSource();
    }

    private void Start()
    {
        RegisterWithAudioManager();
        currentSceneName = SceneManager.GetActiveScene().name;
        
        if (autoSwitchByScene)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        if (playOnStart)
        {
            PlayMusicForCurrentScene();
        }
    }

    private void OnDestroy()
    {
        if (autoSwitchByScene)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoSwitchByScene)
        {
            currentSceneName = scene.name;
            PlayMusicForCurrentScene();
        }
    }

    private void PlayMusicForCurrentScene()
    {
        AudioClip targetClip = null;
        bool shouldLoop = true;
        float startTime = 0f;

        // Önce listede bu sahne için müzik var mı kontrol et
        foreach (var track in musicTracks)
        {
            if (track != null && track.sceneName == currentSceneName && track.clip != null)
            {
                targetClip = track.clip;
                shouldLoop = track.loop;
                startTime = track.startTime;
                break;
            }
        }

        // Eğer listede bulunamazsa, default müzikleri kullan
        if (targetClip == null)
        {
            // Menü sahnesi mi kontrol et (basit string kontrolü)
            if (currentSceneName.ToLower().Contains("menu") || 
                currentSceneName.ToLower().Contains("baslangic") ||
                currentSceneName.ToLower().Contains("start"))
            {
                targetClip = defaultMenuMusic;
            }
            else
            {
                targetClip = defaultGameMusic;
            }
        }

        if (targetClip != null)
        {
            PlayMusic(targetClip, shouldLoop, startTime);
        }
    }

    private void OnEnable()
    {
        RegisterWithAudioManager();
    }

    private void OnDisable()
    {
        UnregisterFromAudioManager();
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true; // Default loop, her müzik kendi loop ayarına sahip
        audioSource.spatialBlend = 0f; // 2D sound
    }

    private void RegisterWithAudioManager()
    {
        if (audioSource == null || AudioSettingsManager.Instance == null || isRegistered)
        {
            return;
        }

        AudioSettingsManager.Instance.RegisterMusicSource(audioSource);
        isRegistered = true;
    }

    private void UnregisterFromAudioManager()
    {
        if (!isRegistered || audioSource == null || AudioSettingsManager.Instance == null)
        {
            isRegistered = false;
            return;
        }

        AudioSettingsManager.Instance.UnregisterSource(audioSource);
        isRegistered = false;
    }

    /// <summary>
    /// Plays the specified music clip.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool shouldLoop = true, float startTime = 0f)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        // Start time'ı clip uzunluğu ile sınırla
        if (startTime < 0f)
        {
            startTime = 0f;
        }
        if (startTime > clip.length)
        {
            startTime = clip.length;
        }

        // Eğer aynı müzik zaten çalıyorsa ve aynı start time ile başlatılıyorsa, değiştirme
        if (currentPlayingClip == clip && audioSource.isPlaying && Mathf.Approximately(audioSource.time, startTime))
        {
            return;
        }

        // Müzik değişiyor, eski müziği durdur
        if (audioSource.isPlaying && currentPlayingClip != clip)
        {
            audioSource.Stop();
        }

        currentPlayingClip = clip;
        audioSource.clip = clip;
        audioSource.loop = shouldLoop;
        audioSource.Play();
        
        // Start time'ı ayarla (Play'den sonra ayarlanmalı)
        if (startTime > 0f)
        {
            audioSource.time = startTime;
        }
    }

    /// <summary>
    /// Plays the menu music and stops any currently playing music.
    /// </summary>
    public void PlayMenuMusic()
    {
        if (defaultMenuMusic != null)
        {
            PlayMusic(defaultMenuMusic);
        }
    }

    /// <summary>
    /// Plays the game music and stops any currently playing music.
    /// </summary>
    public void PlayGameMusic()
    {
        if (defaultGameMusic != null)
        {
            PlayMusic(defaultGameMusic);
        }
    }

    /// <summary>
    /// Plays music for a specific scene by name.
    /// </summary>
    public void PlayMusicForScene(string sceneName)
    {
        foreach (var track in musicTracks)
        {
            if (track != null && track.sceneName == sceneName && track.clip != null)
            {
                PlayMusic(track.clip, track.loop, track.startTime);
                return;
            }
        }

        Debug.LogWarning($"[BackgroundMusicPlayer] No music track found for scene: {sceneName}", this);
    }

    /// <summary>
    /// Adds or updates a music track for a scene.
    /// </summary>
    public void SetMusicForScene(string sceneName, AudioClip clip, bool shouldLoop = true, float startTime = 0f)
    {
        if (string.IsNullOrEmpty(sceneName) || clip == null)
        {
            return;
        }

        // Start time'ı clip uzunluğu ile sınırla
        if (startTime < 0f)
        {
            startTime = 0f;
        }
        if (startTime > clip.length)
        {
            startTime = clip.length;
        }

        // Var olan track'ı güncelle
        foreach (var track in musicTracks)
        {
            if (track != null && track.sceneName == sceneName)
            {
                track.clip = clip;
                track.loop = shouldLoop;
                track.startTime = startTime;
                
                // Eğer bu sahne şu anki sahne ise, müziği değiştir
                if (currentSceneName == sceneName)
                {
                    PlayMusic(clip, shouldLoop, startTime);
                }
                return;
            }
        }

        // Yeni track ekle
        musicTracks.Add(new MusicTrack
        {
            sceneName = sceneName,
            clip = clip,
            loop = shouldLoop,
            startTime = startTime
        });
    }

    /// <summary>
    /// Stops the music.
    /// </summary>
    public void StopMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
            currentPlayingClip = null;
        }
    }

    /// <summary>
    /// Pauses the music.
    /// </summary>
    public void PauseMusic()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Resumes the paused music.
    /// </summary>
    public void ResumeMusic()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }

    /// <summary>
    /// Sets whether the music should loop.
    /// </summary>
    public void SetLoop(bool shouldLoop)
    {
        if (audioSource != null)
        {
            audioSource.loop = shouldLoop;
        }
    }

    /// <summary>
    /// Gets the currently playing clip.
    /// </summary>
    public AudioClip GetCurrentClip()
    {
        return currentPlayingClip;
    }

    /// <summary>
    /// Checks if music is currently playing.
    /// </summary>
    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}

