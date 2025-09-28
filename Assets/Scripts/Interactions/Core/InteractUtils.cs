using UnityEngine;

namespace Interactions.Core
{
    public static class InteractUtils
    {
        public static void SetActiveSafe(Transform target, bool value)
        {
            if (!target)
                return;
            if (target.gameObject.activeSelf != value)
                target.gameObject.SetActive(value);
        }

        public static void SetRendererEnabled(Renderer renderer, bool value)
        {
            if (!renderer)
                return;
            if (renderer.enabled != value)
                renderer.enabled = value;
        }

        public static void SetSpriteEnabled(SpriteRenderer renderer, bool value)
        {
            if (!renderer)
                return;
            if (renderer.enabled != value)
                renderer.enabled = value;
        }

        public static void SetColliderPassable(Collider2D collider, bool passable)
        {
            if (!collider)
                return;
            if (collider.isTrigger != passable)
                collider.isTrigger = passable;
        }

        public static void SetCanvasGroupVisible(CanvasGroup canvasGroup, bool visible, float alphaOn = 1f, float alphaOff = 0f)
        {
            if (!canvasGroup)
                return;

            canvasGroup.alpha = visible ? alphaOn : alphaOff;
            canvasGroup.blocksRaycasts = visible;
            canvasGroup.interactable = visible;
        }

        public static void PlayParticleSafe(ParticleSystem particles)
        {
            if (!particles)
                return;
            if (!particles.isPlaying)
                particles.Play();
        }

        public static void StopParticleSafe(ParticleSystem particles)
        {
            if (!particles)
                return;
            if (particles.isPlaying)
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public static void SetLightEnabled(Light light, bool value)
        {
            if (!light)
                return;
            if (light.enabled != value)
                light.enabled = value;
        }
    }
}
