using UnityEngine;
using UnityEngine.Playables;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Cutscene Trigger Action")]
public class CutsceneTriggerAction : InteractActionBase
{
    [Header("Cutscene")]
    [SerializeField] PlayableDirector playableDirector;
    [SerializeField] bool restartIfPlaying = true;

    protected override void OnComplete(InteractionController controller)
    {
        if (!playableDirector)
            return;

        if (playableDirector.state == PlayState.Playing)
        {
            if (restartIfPlaying)
            {
                playableDirector.time = 0d;
                playableDirector.Play();
            }
        }
        else
        {
            playableDirector.Play();
        }
    }
}
