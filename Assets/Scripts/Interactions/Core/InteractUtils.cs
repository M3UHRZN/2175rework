using UnityEngine;

public static class InteractUtils
{
    public static void SetActiveSafe(Transform target, bool value)
    {
        if (!target)
            return;
        if (target.gameObject.activeSelf != value)
            target.gameObject.SetActive(value);
    }

    public static void SetActiveSafe(GameObject target, bool value)
    {
        if (!target)
            return;
        if (target.activeSelf != value)
            target.SetActive(value);
    }

    public static void SetSpriteEnabled(SpriteRenderer renderer, bool value)
    {
        if (!renderer)
            return;
        renderer.enabled = value;
    }

    public static void SetColliderPassable(Collider2D collider, bool passable)
    {
        if (!collider)
            return;
        collider.isTrigger = passable;
        collider.enabled = true;
    }

    public static void SetAnimatorBool(Animator animator, int parameterId, bool value)
    {
        if (!animator)
            return;
        animator.SetBool(parameterId, value);
    }

    public static void PlayAnimatorTrigger(Animator animator, int parameterId)
    {
        if (!animator)
            return;
        animator.ResetTrigger(parameterId);
        animator.SetTrigger(parameterId);
    }

    public static void PlayParticle(ParticleSystem ps)
    {
        if (!ps)
            return;
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Play();
    }

    public static void SetCanvasGroupAlpha(CanvasGroup group, float alpha)
    {
        if (!group)
            return;
        group.alpha = Mathf.Clamp01(alpha);
        group.blocksRaycasts = alpha > 0f;
        group.interactable = alpha > 0.95f;
    }
}
