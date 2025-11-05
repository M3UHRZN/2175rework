using System.Collections.Generic;
using UnityEngine;
using Game.Settings;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(LocomotionMotor2D))]
[RequireComponent(typeof(Sensors2D))]
public class CharacterAudioController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private LocomotionMotor2D locomotionMotor;
    [SerializeField] private Sensors2D sensors;

    [Header("Ground Movement")]
    [SerializeField] private AudioSource runFootstepSource;
    [SerializeField] private AudioClip runFootstepClip;
    [SerializeField] private float footstepSpeedThreshold = 0.25f;

    [Header("Climb")]
    [SerializeField] private AudioSource climbLoopSource;

    [Header("Wall Movement")]
    [SerializeField] private AudioSource wallClimbLoopSource;
    [SerializeField] private AudioSource wallSlideLoopSource;

    [Header("Airborne")]
    [SerializeField] private AudioSource jumpSource;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private float jumpMinInterval = 0.1f;
    [SerializeField] private AudioSource landSource;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private float landMinInterval = 0.1f;
    [SerializeField] private AudioSource wallJumpSource;
    [SerializeField] private AudioClip wallJumpClip;
    [SerializeField] private float wallJumpMinInterval = 0.1f;

    private readonly List<AudioSource> registeredSources = new();
    private float lastJumpTime = float.NegativeInfinity;
    private float lastLandTime = float.NegativeInfinity;
    private float lastWallJumpTime = float.NegativeInfinity;

    private void Awake()
    {
        if (!stateMachine)
        {
            stateMachine = GetComponent<PlayerStateMachine>();
        }

        if (!locomotionMotor)
        {
            locomotionMotor = GetComponent<LocomotionMotor2D>();
        }

        if (!sensors)
        {
            sensors = GetComponent<Sensors2D>();
        }
    }

    private void OnEnable()
    {
        if (stateMachine != null)
        {
            stateMachine.OnLocoChanged += HandleLocoChanged;
            stateMachine.OnPhaseTriggered += HandlePhaseTriggered;
        }

        RegisterSources();
        SyncLoopStates(stateMachine != null ? stateMachine.Current : PlayerStateMachine.LocoState.Idle);
    }

    private void OnDisable()
    {
        if (stateMachine != null)
        {
            stateMachine.OnLocoChanged -= HandleLocoChanged;
            stateMachine.OnPhaseTriggered -= HandlePhaseTriggered;
        }

        UnregisterSources();
        StopLoop(runFootstepSource);
        StopLoop(climbLoopSource);
        StopLoop(wallClimbLoopSource);
        StopLoop(wallSlideLoopSource);
    }

    private void Update()
    {
        UpdateFootsteps();
    }

    private void HandleLocoChanged(PlayerStateMachine.LocoState previous, PlayerStateMachine.LocoState current)
    {
        switch (previous)
        {
            case PlayerStateMachine.LocoState.Climb:
                StopLoop(climbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                StopLoop(wallClimbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                StopLoop(wallSlideLoopSource);
                break;
        }

        switch (current)
        {
            case PlayerStateMachine.LocoState.Climb:
                StartLoop(climbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                StartLoop(wallClimbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                StartLoop(wallSlideLoopSource);
                break;
            case PlayerStateMachine.LocoState.JumpRise:
                TryPlayOneShot(jumpSource, jumpClip, ref lastJumpTime, jumpMinInterval);
                break;
        }

        if (IsLandingTransition(previous, current))
        {
            TryPlayOneShot(landSource, landClip, ref lastLandTime, landMinInterval);
        }
    }

    private void HandlePhaseTriggered(PlayerStateMachine.PhaseState phase)
    {
        if (phase == PlayerStateMachine.PhaseState.WallJump)
        {
            TryPlayOneShot(wallJumpSource, wallJumpClip, ref lastWallJumpTime, wallJumpMinInterval);
        }
    }

    private void UpdateFootsteps()
    {
        if (stateMachine == null || locomotionMotor == null || sensors == null)
        {
            return;
        }

        var shouldLoop = stateMachine.Current == PlayerStateMachine.LocoState.Run &&
                         sensors.isGrounded &&
                         Mathf.Abs(locomotionMotor.velocityX) >= footstepSpeedThreshold;

        if (runFootstepSource == null)
        {
            return;
        }

        if (shouldLoop)
        {
            if (runFootstepClip != null && runFootstepSource.clip != runFootstepClip)
            {
                runFootstepSource.clip = runFootstepClip;
            }

            runFootstepSource.loop = true;

            if (!runFootstepSource.isPlaying)
            {
                runFootstepSource.Play();
            }
        }
        else if (runFootstepSource.isPlaying)
        {
            runFootstepSource.Stop();
        }
    }

    private bool TryPlayOneShot(AudioSource source, AudioClip clip, ref float lastTime, float minInterval)
    {
        if (source == null)
        {
            return false;
        }

        if (Time.time < lastTime + minInterval)
        {
            return false;
        }

        if (clip != null)
        {
            source.PlayOneShot(clip);
        }
        else if (!source.isPlaying)
        {
            source.Play();
        }

        lastTime = Time.time;
        return true;
    }

    private static void StartLoop(AudioSource source)
    {
        if (source != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    private static void StopLoop(AudioSource source)
    {
        if (source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    private bool IsLandingTransition(PlayerStateMachine.LocoState previous, PlayerStateMachine.LocoState current)
    {
        if (current != PlayerStateMachine.LocoState.Run && current != PlayerStateMachine.LocoState.Idle)
        {
            return false;
        }

        switch (previous)
        {
            case PlayerStateMachine.LocoState.JumpRise:
            case PlayerStateMachine.LocoState.JumpFall:
            case PlayerStateMachine.LocoState.WallSlide:
            case PlayerStateMachine.LocoState.WallClimb:
            case PlayerStateMachine.LocoState.Climb:
                return true;
            default:
                return false;
        }
    }

    private void RegisterSources()
    {
        registeredSources.Clear();
        foreach (var source in EnumerateSources())
        {
            if (source == null || registeredSources.Contains(source))
            {
                continue;
            }

            registeredSources.Add(source);
            if (AudioSettingsManager.Instance != null)
            {
                AudioSettingsManager.Instance.RegisterSfxSource(source);
            }
        }
    }

    private void UnregisterSources()
    {
        if (AudioSettingsManager.Instance == null)
        {
            registeredSources.Clear();
            return;
        }

        foreach (var source in registeredSources)
        {
            AudioSettingsManager.Instance.UnregisterSource(source);
        }

        registeredSources.Clear();
    }

    private IEnumerable<AudioSource> EnumerateSources()
    {
        if (runFootstepSource != null) yield return runFootstepSource;
        if (climbLoopSource != null) yield return climbLoopSource;
        if (wallClimbLoopSource != null) yield return wallClimbLoopSource;
        if (wallSlideLoopSource != null) yield return wallSlideLoopSource;
        if (jumpSource != null) yield return jumpSource;
        if (landSource != null) yield return landSource;
        if (wallJumpSource != null) yield return wallJumpSource;
    }

    private void SyncLoopStates(PlayerStateMachine.LocoState current)
    {
        switch (current)
        {
            case PlayerStateMachine.LocoState.Climb:
                StartLoop(climbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                StartLoop(wallClimbLoopSource);
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                StartLoop(wallSlideLoopSource);
                break;
        }
    }
}
