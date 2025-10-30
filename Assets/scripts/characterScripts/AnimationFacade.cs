using UnityEngine;

[DisallowMultipleComponent]
public class AnimationFacade : MonoBehaviour
{
    public Animator animator;
    public void SetBool(string n, bool v){ if(animator) animator.SetBool(n,v); }
    public void SetFloat(string n, float v){ if(animator) animator.SetFloat(n,v); }
    public void SetInt(string n, int v){ if(animator) animator.SetInteger(n,v); }
    public void Trigger(string n){ if(animator) animator.SetTrigger(n); }
    public void ResetTrigger(string n){ if(animator) animator.ResetTrigger(n); }
}
