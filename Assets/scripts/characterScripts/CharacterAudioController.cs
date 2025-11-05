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
    [Tooltip("Minimum seconds to keep the run loop active once it starts.")]
    [SerializeField] private float runLoopMinDuration = 0.1f;

    [Header("Climb")]
    [SerializeField] private AudioSource climbLoopSource;
    [SerializeField] private AudioClip climbLoopClip;
    [Tooltip("Minimum seconds to keep the climb loop active once it starts.")]
    [SerializeField] private float climbLoopMinDuration = 0.1f;

    [Header("Wall Movement")]
    [SerializeField] private AudioSource wallClimbLoopSource;
    [SerializeField] private AudioClip wallClimbLoopClip;
    [SerializeField] private AudioSource wallSlideLoopSource;
    [SerializeField] private AudioClip wallSlideLoopClip;
    [Tooltip("Minimum seconds to keep the wall climb loop active once it starts.")]
    [SerializeField] private float wallClimbLoopMinDuration = 0.1f;
    [Tooltip("Minimum seconds to keep the wall slide loop active once it starts.")]
    [SerializeField] private float wallSlideLoopMinDuration = 0.1f;

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
    private readonly Dictionary<AudioSource, LoopRuntimeState> loopStates = new();
    private readonly List<AudioSource> loopStateKeys = new();
    private float lastJumpTime = float.NegativeInfinity;
    private float lastLandTime = float.NegativeInfinity;
    private float lastWallJumpTime = float.NegativeInfinity;

    private struct LoopRuntimeState
    {
        public float LastStartTime;
        public float MinDuration;
        public bool PendingStop;
    }

    private void Awake()
    {
        CacheDependencies();

        if (Application.isPlaying)
        {
            EnsureAudioSources();
        }
    }

    private void OnEnable()
    {
        CacheDependencies();

        if (!Application.isPlaying)
        {
            return;
        }

        EnsureAudioSources();

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
        if (!Application.isPlaying)
        {
            return;
        }

        if (stateMachine != null)
        {
            stateMachine.OnLocoChanged -= HandleLocoChanged;
            stateMachine.OnPhaseTriggered -= HandlePhaseTriggered;
        }

        UnregisterSources();
        StopLoop(runFootstepSource, true);
        StopLoop(climbLoopSource, true);
        StopLoop(wallClimbLoopSource, true);
        StopLoop(wallSlideLoopSource, true);

        loopStates.Clear();
        loopStateKeys.Clear();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        UpdateFootsteps();
        UpdatePendingLoopStops();
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
                StartLoop(climbLoopSource, climbLoopClip, climbLoopMinDuration);
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                StartLoop(wallClimbLoopSource, wallClimbLoopClip, wallClimbLoopMinDuration);
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                StartLoop(wallSlideLoopSource, wallSlideLoopClip, wallSlideLoopMinDuration);
                break;
            case PlayerStateMachine.LocoState.JumpRise:
                TryPlayClip(jumpSource, jumpClip, ref lastJumpTime, jumpMinInterval);
                break;
        }

        if (IsLandingTransition(previous, current))
        {
            TryPlayClip(landSource, landClip, ref lastLandTime, landMinInterval);
        }
    }

    private void HandlePhaseTriggered(PlayerStateMachine.PhaseState phase)
    {
        if (phase == PlayerStateMachine.PhaseState.WallJump)
        {
            TryPlayClip(wallJumpSource, wallJumpClip, ref lastWallJumpTime, wallJumpMinInterval);
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
            StartLoop(runFootstepSource, runFootstepClip, runLoopMinDuration);
        }
        else
        {
            StopLoop(runFootstepSource);
        }
    }

    private bool TryPlayClip(AudioSource source, AudioClip clip, ref float lastTime, float minInterval)
    {
        if (source == null)
        {
            return false;
        }

        if (Time.time < lastTime + minInterval)
        {
            return false;
        }

        if (clip != null && source.clip != clip)
        {
            source.clip = clip;
        }

        if (source.clip == null)
        {
            return false;
        }

        source.loop = false;

        if (source.isPlaying)
        {
            source.Stop();
        }

        source.Play();

        lastTime = Time.time;
        return true;
    }

    private void StartLoop(AudioSource source, AudioClip clip, float minDuration)
    {
        if (source == null)
        {
            return;
        }

        bool clipChanged = false;

        if (clip != null && source.clip != clip)
        {
            clipChanged = true;
            source.clip = clip;
        }

        if (source.clip == null)
        {
            return;
        }

        source.loop = true;

        if (clipChanged && source.isPlaying)
        {
            source.Stop();
        }

        var state = loopStates.TryGetValue(source, out var existingState) ? existingState : new LoopRuntimeState
        {
            LastStartTime = float.NegativeInfinity
        };

        state.MinDuration = Mathf.Max(0f, minDuration);
        state.PendingStop = false;

        if (!source.isPlaying)
        {
            source.Play();
            state.LastStartTime = Time.time;
        }
        else if (float.IsNegativeInfinity(state.LastStartTime))
        {
            state.LastStartTime = Time.time;
        }

        loopStates[source] = state;
    }

    private void StopLoop(AudioSource source, bool immediate = false)
    {
        if (source == null)
        {
            return;
        }

        if (!loopStates.TryGetValue(source, out var state))
        {
            state = new LoopRuntimeState
            {
                LastStartTime = float.NegativeInfinity,
                MinDuration = 0f,
                PendingStop = false
            };
        }

        if (immediate || !source.isPlaying || Time.time >= state.LastStartTime + state.MinDuration)
        {
            FinalizeLoopStop(source, ref state);
        }
        else
        {
            state.PendingStop = true;
            loopStates[source] = state;
        }
    }

    private void FinalizeLoopStop(AudioSource source, ref LoopRuntimeState state)
    {
        source.loop = false;

        if (source.isPlaying)
        {
            source.Stop();
        }

        state.PendingStop = false;
        state.LastStartTime = float.NegativeInfinity;
        loopStates[source] = state;
    }

    private void UpdatePendingLoopStops()
    {
        if (loopStates.Count == 0)
        {
            return;
        }

        loopStateKeys.Clear();
        foreach (var key in loopStates.Keys)
        {
            loopStateKeys.Add(key);
        }

        foreach (var source in loopStateKeys)
        {
            if (source == null)
            {
                loopStates.Remove(source);
                continue;
            }

            var state = loopStates[source];

            if (!state.PendingStop)
            {
                continue;
            }

            if (!source.isPlaying || Time.time >= state.LastStartTime + state.MinDuration)
            {
                FinalizeLoopStop(source, ref state);
            }
            else
            {
                loopStates[source] = state;
            }
        }

        loopStateKeys.Clear();
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
                StartLoop(climbLoopSource, climbLoopClip, climbLoopMinDuration);
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                StartLoop(wallClimbLoopSource, wallClimbLoopClip, wallClimbLoopMinDuration);
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                StartLoop(wallSlideLoopSource, wallSlideLoopClip, wallSlideLoopMinDuration);
                break;
        }
    }

    private void CacheDependencies()
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

    private void EnsureAudioSources()
    {
        EnsureSource(ref runFootstepSource, "Run Footsteps");
        EnsureSource(ref climbLoopSource, "Climb Loop");
        EnsureSource(ref wallClimbLoopSource, "Wall Climb Loop");
        EnsureSource(ref wallSlideLoopSource, "Wall Slide Loop");
        EnsureSource(ref jumpSource, "Jump");
        EnsureSource(ref landSource, "Land");
        EnsureSource(ref wallJumpSource, "Wall Jump");
    }

    private void EnsureSource(ref AudioSource source, string childName)
    {
        if (source == null)
        {
            source = CreateChildSource(childName);
        }
    }

    private AudioSource CreateChildSource(string childName)
    {
        var child = new GameObject(childName);
        child.transform.SetParent(transform);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;
        child.hideFlags = HideFlags.DontSave;

        var source = child.AddComponent<AudioSource>();
        ConfigureSourceDefaults(source);
        return source;
    }

    private void ConfigureSourceDefaults(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
    }
}
