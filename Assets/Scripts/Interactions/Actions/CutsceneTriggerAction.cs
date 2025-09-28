using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[AddComponentMenu("Interactions/Actions/Cutscene Trigger Action")]
public class CutsceneTriggerAction : InteractActionBase
{
    [SerializeField] PlayableDirector director;
    [SerializeField] UnityEvent onTriggered;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (director)
        {
            director.time = 0;
            director.Play();
        }
        onTriggered?.Invoke();
    }
}
