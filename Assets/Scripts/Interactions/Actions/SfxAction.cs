using UnityEngine;

[AddComponentMenu("Interactions/Actions/SFX Action")]
public class SfxAction : InteractActionBase
{
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip onStartClip;
    [SerializeField] AudioClip onCompleteClip;
    [SerializeField] AudioClip onCancelClip;
    [SerializeField] float volume = 1f;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        Play(onStartClip);
    }

    protected override void OnComplete(InteractionController controller)
    {
        Play(onCompleteClip);
    }

    protected override void OnCancel(InteractionController controller)
    {
        Play(onCancelClip);
    }

    void Play(AudioClip clip)
    {
        if (!clip)
            return;
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();
        audioSource?.PlayOneShot(clip, volume);
    }
}
