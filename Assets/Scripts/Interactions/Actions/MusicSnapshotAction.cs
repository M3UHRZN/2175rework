using UnityEngine;
using UnityEngine.Audio;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Music Snapshot Action")]
public class MusicSnapshotAction : InteractActionBase
{
    [Header("Music Snapshot")]
    [SerializeField] AudioMixerSnapshot snapshot;
    [SerializeField] float transitionTime = 0.5f;

    protected override void OnComplete(InteractionController controller)
    {
        if (snapshot)
        {
            snapshot.TransitionTo(Mathf.Max(0f, transitionTime));
        }
    }
}
