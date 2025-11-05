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
    [SerializeField] private float footstepMaxSpeed = 7.5f;
    [SerializeField] private float footstepMinInterval = 0.18f;
    [SerializeField] private float footstepMaxInterval = 0.45f;

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
    private float footstepTimer;
    private float lastFootstepTime = float.NegativeInfinity;
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
        StopLoop(climbLoopSource);
        StopLoop(wallClimbLoopSource);
        StopLoop(wallSlideLoopSource);
    }

    private void Update()
    {
        UpdateFootsteps(Time.deltaTime);
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

    private void UpdateFootsteps(float deltaTime)
    {
        if (stateMachine == null || locomotionMotor == null || sensors == null)
        {
            return;
        }

        if (stateMachine.Current != PlayerStateMachine.LocoState.Run || !sensors.isGrounded)
        {
            footstepTimer = 0f;
            return;
        }

        var speed = Mathf.Abs(locomotionMotor.velocityX);
        if (speed < footstepSpeedThreshold)
        {
            footstepTimer = 0f;
            return;
        }

        var interval = Mathf.Lerp(footstepMaxInterval, footstepMinInterval,
            Mathf.Clamp01((speed - footstepSpeedThreshold) / Mathf.Max(0.01f, footstepMaxSpeed - footstepSpeedThreshold)));

        footstepTimer += deltaTime;
        if (footstepTimer >= interval && Time.time >= lastFootstepTime + footstepMinInterval)
        {
            if (TryPlayOneShot(runFootstepSource, runFootstepClip, ref lastFootstepTime, footstepMinInterval))
            {
                footstepTimer = 0f;
            }
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
