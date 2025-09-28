using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Interactions/Actions/Music Snapshot Action")]
public class MusicSnapshotAction : InteractActionBase
{
    [SerializeField] AudioMixerSnapshot snapshot;
    [SerializeField] float transitionTime = 0.5f;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        snapshot?.TransitionTo(Mathf.Max(0f, transitionTime));
    }
}
