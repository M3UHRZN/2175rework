using UnityEngine;
using Interactions.Core;

[RequireComponent(typeof(Interactable))]
[AddComponentMenu("Interactions/Actions/Particle Burst Action")]
public class ParticleBurstAction : InteractActionBase
{
    [Header("Particles")]
    [SerializeField] ParticleSystem particleSystem;

    protected override void OnComplete(InteractionController controller)
    {
        InteractUtils.PlayParticleSafe(particleSystem);
    }
}
