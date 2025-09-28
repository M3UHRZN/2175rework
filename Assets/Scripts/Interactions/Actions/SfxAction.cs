using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/SFX Action")]
public class SfxAction : InteractActionBase
{
    [Header("Sound Effects")]
    [SerializeField] AudioSource onStart;
    [SerializeField] AudioSource onComplete;
    [SerializeField] AudioSource onCancel;

    protected override void Awake()
    {
        base.Awake();
        SetEventListening(true, false, true, true);
    }

    protected override void OnStart(InteractionController controller)
    {
        Play(onStart);
    }

    protected override void OnComplete(InteractionController controller)
    {
        Play(onComplete);
    }

    protected override void OnCancel(InteractionController controller)
    {
        Play(onCancel);
    }

    void Play(AudioSource source)
    {
        if (!source)
            return;
        source.Play();
    }
}
