using System.Collections.Generic;
using Game.Settings;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayerInteractable : Interactable
{
    [Header("Music Player")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> playlist = new();
    [SerializeField] private bool loopPlaylist = true;
    [Tooltip("When true, playback starts from the first track when the scene loads.")]
    [SerializeField] private bool playOnStart;

    private int currentTrackIndex;
    private bool isActive;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void Awake()
    {
        audioSource ??= GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }

    private void OnEnable()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        TryRegister();

        OnInteractComplete.RemoveListener(HandleInteractionComplete);
        OnInteractComplete.AddListener(HandleInteractionComplete);

        if (playOnStart && !isActive)
        {
            PlayCurrentTrack();
        }
    }

    private void OnDisable()
    {
        var manager = AudioSettingsManager.Instance;
        if (manager != null && audioSource != null)
        {
            manager.UnregisterSource(audioSource);
        }

        OnInteractComplete.RemoveListener(HandleInteractionComplete);
    }

    private void HandleInteractionComplete(InteractionController controller)
    {
        if (playlist == null || playlist.Count == 0 || audioSource == null)
        {
            return;
        }

        if (!isActive)
        {
            PlayCurrentTrack();
            return;
        }

        if (playlist.Count == 1)
        {
            StopPlayback();
            return;
        }

        if (TryAdvanceTrack())
        {
            PlayCurrentTrack();
        }
        else
        {
            currentTrackIndex = 0;
            StopPlayback();
        }
    }

    private bool TryAdvanceTrack()
    {
        if (playlist == null || playlist.Count == 0)
        {
            return false;
        }

        var nextIndex = currentTrackIndex + 1;
        if (nextIndex >= playlist.Count)
        {
            if (!loopPlaylist)
            {
                return false;
            }

            nextIndex = 0;
        }

        currentTrackIndex = nextIndex;
        return true;
    }

    private void PlayCurrentTrack()
    {
        if (playlist == null || playlist.Count == 0 || audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = playlist[Mathf.Clamp(currentTrackIndex, 0, playlist.Count - 1)];
        TryRegister();
        audioSource.Play();
        isActive = true;
    }

    private void StopPlayback()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        isActive = false;
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
