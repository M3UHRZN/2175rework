using UnityEngine;

[AddComponentMenu("Interactions/Actions/Particle Burst Action")]
public class ParticleBurstAction : InteractActionBase
{
    [SerializeField] ParticleSystem particleSystem;

    protected override void Reset()
    {
        base.Reset();
        SetDefaultListeners(false, false, true, false);
    }

    protected override void OnComplete(InteractionController controller)
    {
        if (!particleSystem)
            particleSystem = GetComponent<ParticleSystem>();
        InteractUtils.PlayParticle(particleSystem);
    }
}
