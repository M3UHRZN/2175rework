using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Teleports characters back to a respawn location when they collide with the trap (eg. spikes).
/// Supports optional ease-out movement and filtering which character is affected.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SpikeTrap : MonoBehaviour
{
    public enum TrapTarget
    {
        Elior,
        Sim,
        Both
    }

    [Header("Trap Setup")]
    [Tooltip("Which characters are affected by this trap.")]
    [SerializeField] TrapTarget affectedCharacters = TrapTarget.Both;

    [Tooltip("Respawn location used when no character specific override is assigned.")]
    [SerializeField] Transform fallbackRespawnPoint;

    [Tooltip("Optional respawn location override for Elior.")]
    [SerializeField] Transform eliorRespawnPoint;

    [Tooltip("Optional respawn location override for Sim.")]
    [SerializeField] Transform simRespawnPoint;

    [Header("Characters")]
    [Tooltip("Reference to Elior's orchestrator component.")]
    [SerializeField] PlayerOrchestrator elior;

    [Tooltip("Reference to Sim's orchestrator component.")]
    [SerializeField] PlayerOrchestrator sim;

    [Header("Ease Out Effect")]
    [Tooltip("Enable a short ease-out animation instead of an instant teleport.")]
    [SerializeField] bool useEaseOutEffect = false;

    [Tooltip("Duration of the ease-out animation in seconds.")]
    [SerializeField, Min(0f)] float easeDuration = 0.3f;

    [Tooltip("Curve used to interpolate between the start and respawn positions when ease-out is enabled.")]
    [SerializeField] AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    readonly Dictionary<PlayerOrchestrator, Coroutine> activeRoutines = new Dictionary<PlayerOrchestrator, Coroutine>();

    Collider2D cachedCollider;

    void Reset()
    {
        cachedCollider = GetComponent<Collider2D>();
        if (cachedCollider)
        {
            cachedCollider.isTrigger = true;
        }

        AutoAssignCharacters();
    }

    void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
        if (cachedCollider && !cachedCollider.isTrigger)
        {
            Debug.LogWarning($"[SpikeTrap] Collider on {name} is not configured as a trigger. Enabling trigger mode automatically.", this);
            cachedCollider.isTrigger = true;
        }

        AutoAssignCharacters();
    }

    void OnDisable()
    {
        foreach (var routine in activeRoutines.Values)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        activeRoutines.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleHit(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider)
        {
            TryHandleHit(collision.collider);
        }
    }

    void TryHandleHit(Collider2D other)
    {
        var actor = ResolveActor(other);
        if (!actor)
        {
            return;
        }

        if (!IsActorAffected(actor))
        {
            return;
        }

        BeginRespawn(actor);
    }

    void BeginRespawn(PlayerOrchestrator actor)
    {
        if (!actor)
        {
            return;
        }

        if (activeRoutines.TryGetValue(actor, out var routine) && routine != null)
        {
            StopCoroutine(routine);
        }

        var newRoutine = StartCoroutine(RespawnRoutine(actor));
        activeRoutines[actor] = newRoutine;
    }

    IEnumerator RespawnRoutine(PlayerOrchestrator actor)
    {
        var targetPosition = ResolveRespawnPosition(actor);
        if (!targetPosition.HasValue)
        {
            Debug.LogWarning($"[SpikeTrap] No respawn point configured for {actor.name}.", this);
            activeRoutines[actor] = null;
            yield break;
        }

        var body = actor.GetComponent<Rigidbody2D>();
        if (body)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }

        var transform = actor.transform;

        if (!useEaseOutEffect || easeDuration <= 0f)
        {
            if (body)
            {
                bool wasSimulated = body.simulated;
                if (!wasSimulated)
                {
                    body.simulated = true;
                }

                body.position = targetPosition.Value;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;

                if (!wasSimulated)
                {
                    body.simulated = false;
                }
            }
            else
            {
                transform.position = targetPosition.Value;
            }
        }
        else
        {
            bool restoreSimulation = false;
            if (body && body.simulated)
            {
                body.simulated = false;
                restoreSimulation = true;
            }

            Vector3 start = transform.position;
            Vector3 end = targetPosition.Value;
            float elapsed = 0f;

            while (elapsed < easeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / easeDuration);
                float eased = easeCurve != null ? easeCurve.Evaluate(t) : t;
                transform.position = Vector3.Lerp(start, end, eased);
                yield return null;
            }

            transform.position = end;

            if (body)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                if (restoreSimulation)
                {
                    body.simulated = true;
                }
            }
        }

        activeRoutines.Remove(actor);
    }

    Vector2? ResolveRespawnPosition(PlayerOrchestrator actor)
    {
        if (!actor)
        {
            return null;
        }

        if (actor == elior && eliorRespawnPoint)
        {
            return eliorRespawnPoint.position;
        }

        if (actor == sim && simRespawnPoint)
        {
            return simRespawnPoint.position;
        }

        if (fallbackRespawnPoint)
        {
            return fallbackRespawnPoint.position;
        }

        return null;
    }

    bool IsActorAffected(PlayerOrchestrator actor)
    {
        if (!actor)
        {
            return false;
        }

        switch (affectedCharacters)
        {
            case TrapTarget.Elior:
                return actor == elior;
            case TrapTarget.Sim:
                return actor == sim;
            case TrapTarget.Both:
                return actor == elior || actor == sim;
            default:
                return false;
        }
    }

    PlayerOrchestrator ResolveActor(Collider2D collider)
    {
        if (!collider)
        {
            return null;
        }

        var actor = collider.GetComponent<PlayerOrchestrator>();
        if (actor)
        {
            return actor;
        }

        return collider.GetComponentInParent<PlayerOrchestrator>();
    }

    void AutoAssignCharacters()
    {
        if (elior && sim)
        {
            return;
        }

        var party = FindFirstObjectByType<DualCharacterController>();
        if (party)
        {
            if (!elior)
            {
                elior = party.elior;
            }

            if (!sim)
            {
                sim = party.sim;
            }
        }
        else if (!elior)
        {
            elior = FindFirstObjectByType<PlayerOrchestrator>();
        }
    }
}
