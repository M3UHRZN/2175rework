using UnityEngine;
using Game.Settings;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(LocomotionMotor2D))]
[RequireComponent(typeof(Sensors2D))]
[RequireComponent(typeof(AudioSource))]
public class CharacterAudioController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private LocomotionMotor2D locomotionMotor;
    [SerializeField] private Sensors2D sensors;
    [SerializeField] private AudioSource audioSource;

    [Header("Loop Clips")]
    [SerializeField] private AudioClip runLoopClip;
    [SerializeField, Min(0f)] private float runSpeedThreshold = 0.1f;
    [SerializeField] private AudioClip climbLoopClip;
    [SerializeField] private AudioClip wallClimbLoopClip;
    [SerializeField] private AudioClip wallSlideLoopClip;
    [SerializeField, Min(0f)] private float verticalSpeedThreshold = 0.05f;

    [Header("One-Shot Clips")]
    [SerializeField] private AudioClip jumpClip;
    [SerializeField, Min(0f)] private float jumpMaxDuration = 0.6f;
    [SerializeField] private AudioClip wallJumpClip;
    [SerializeField, Min(0f)] private float wallJumpMaxDuration = 0.6f;
    [SerializeField] private AudioClip landClip;
    [SerializeField, Min(0f)] private float landMaxDuration = 0.45f;

    private enum PlaybackMode
    {
        None,
        RunLoop,
        ClimbLoop,
        WallClimbLoop,
        WallSlideLoop,
        JumpOneShot,
        WallJumpOneShot,
        LandOneShot
    }

    private PlaybackMode currentMode = PlaybackMode.None;
    private float modeStartTime;
    private float currentMaxDuration;
    private bool pendingLand;
    private bool isRegistered;

    private void Reset()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        locomotionMotor = GetComponent<LocomotionMotor2D>();
        sensors = GetComponent<Sensors2D>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        CacheDependencies();
        ConfigureAudioSource();
    }

    private void OnEnable()
    {
        CacheDependencies();
        ConfigureAudioSource();

        if (stateMachine)
        {
            stateMachine.OnLocoChanged += HandleLocoChanged;
            stateMachine.OnPhaseTriggered += HandlePhaseTriggered;
        }

        RegisterAudioSource();
        currentMode = PlaybackMode.None;
        pendingLand = false;
    }

    private void OnDisable()
    {
        if (stateMachine)
        {
            stateMachine.OnLocoChanged -= HandleLocoChanged;
            stateMachine.OnPhaseTriggered -= HandlePhaseTriggered;
        }

        StopAudio(true);
        pendingLand = false;

        UnregisterAudioSource();
    }

    private void Update()
    {
        if (!Application.isPlaying || audioSource == null)
        {
            return;
        }

        if (pendingLand && (sensors == null || sensors.isGrounded))
        {
            PlayOneShot(landClip, PlaybackMode.LandOneShot, landMaxDuration);
            pendingLand = false;
        }

        if (IsOneShot(currentMode))
        {
            if (!audioSource.isPlaying)
            {
                StopAudio(true);
            }
            else if (currentMaxDuration > 0f && Time.time >= modeStartTime + currentMaxDuration)
            {
                audioSource.Stop();
                StopAudio(true);
            }
            else
            {
                return;
            }
        }

        UpdateLoopPlayback();
    }

    private void HandleLocoChanged(PlayerStateMachine.LocoState previous, PlayerStateMachine.LocoState current)
    {
        if (current == PlayerStateMachine.LocoState.JumpRise)
        {
            PlayOneShot(jumpClip, PlaybackMode.JumpOneShot, jumpMaxDuration);
        }

        if (IsLandingTransition(previous, current))
        {
            if (sensors == null || sensors.isGrounded)
            {
                PlayOneShot(landClip, PlaybackMode.LandOneShot, landMaxDuration);
            }
            else if (landClip != null)
            {
                pendingLand = true;
            }
        }
    }

    private void HandlePhaseTriggered(PlayerStateMachine.PhaseState phase)
    {
        if (phase == PlayerStateMachine.PhaseState.WallJump)
        {
            PlayOneShot(wallJumpClip, PlaybackMode.WallJumpOneShot, wallJumpMaxDuration);
        }
    }

    private void UpdateLoopPlayback()
    {
        var desiredMode = DetermineLoopMode();

        if (desiredMode == PlaybackMode.None)
        {
            if (IsLoop(currentMode))
            {
                StopAudio(true);
            }
            return;
        }

        var desiredClip = GetClipForMode(desiredMode);
        if (desiredClip == null)
        {
            if (IsLoop(currentMode))
            {
                StopAudio(true);
            }
            return;
        }

        if (currentMode == desiredMode && audioSource.clip == desiredClip && audioSource.loop)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            return;
        }

        audioSource.loop = true;
        if (audioSource.clip != desiredClip)
        {
            audioSource.Stop();
            audioSource.clip = desiredClip;
        }

        audioSource.Play();
        currentMode = desiredMode;
        modeStartTime = Time.time;
        currentMaxDuration = 0f;
    }

    private PlaybackMode DetermineLoopMode()
    {
        if (stateMachine == null || locomotionMotor == null)
        {
            return PlaybackMode.None;
        }

        switch (stateMachine.Current)
        {
            case PlayerStateMachine.LocoState.Run:
                if (runLoopClip != null && sensors != null && sensors.isGrounded && Mathf.Abs(locomotionMotor.velocityX) > runSpeedThreshold)
                {
                    return PlaybackMode.RunLoop;
                }
                break;
            case PlayerStateMachine.LocoState.Climb:
                if (climbLoopClip != null && Mathf.Abs(locomotionMotor.velocityY) > verticalSpeedThreshold)
                {
                    return PlaybackMode.ClimbLoop;
                }
                break;
            case PlayerStateMachine.LocoState.WallClimb:
                if (wallClimbLoopClip != null && Mathf.Abs(locomotionMotor.velocityY) > verticalSpeedThreshold)
                {
                    return PlaybackMode.WallClimbLoop;
                }
                break;
            case PlayerStateMachine.LocoState.WallSlide:
                if (wallSlideLoopClip != null && Mathf.Abs(locomotionMotor.velocityY) > verticalSpeedThreshold)
                {
                    return PlaybackMode.WallSlideLoop;
                }
                break;
        }

        return PlaybackMode.None;
    }

    private AudioClip GetClipForMode(PlaybackMode mode)
    {
        return mode switch
        {
            PlaybackMode.RunLoop => runLoopClip,
            PlaybackMode.ClimbLoop => climbLoopClip,
            PlaybackMode.WallClimbLoop => wallClimbLoopClip,
            PlaybackMode.WallSlideLoop => wallSlideLoopClip,
            _ => null
        };
    }

    private bool PlayOneShot(AudioClip clip, PlaybackMode mode, float maxDuration)
    {
        if (audioSource == null || clip == null)
        {
            return false;
        }

        audioSource.loop = false;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        currentMode = mode;
        modeStartTime = Time.time;
        currentMaxDuration = maxDuration > 0f ? maxDuration : clip.length;
        pendingLand = false;
        return true;
    }

    private void StopAudio(bool clearClip)
    {
        if (audioSource == null)
        {
            return;
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.loop = false;
        if (clearClip)
        {
            audioSource.clip = null;
        }

        currentMode = PlaybackMode.None;
        currentMaxDuration = 0f;
    }

    private static bool IsOneShot(PlaybackMode mode)
    {
        return mode == PlaybackMode.JumpOneShot || mode == PlaybackMode.WallJumpOneShot || mode == PlaybackMode.LandOneShot;
    }

    private static bool IsLoop(PlaybackMode mode)
    {
        return mode == PlaybackMode.RunLoop || mode == PlaybackMode.ClimbLoop || mode == PlaybackMode.WallClimbLoop || mode == PlaybackMode.WallSlideLoop;
    }

    private bool IsLandingTransition(PlayerStateMachine.LocoState previous, PlayerStateMachine.LocoState current)
    {
        if (current != PlayerStateMachine.LocoState.Idle && current != PlayerStateMachine.LocoState.Run)
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

        if (!audioSource)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            return;
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void RegisterAudioSource()
    {
        if (audioSource == null || AudioSettingsManager.Instance == null || isRegistered)
        {
            return;
        }

        AudioSettingsManager.Instance.RegisterSfxSource(audioSource);
        isRegistered = true;
    }

    private void UnregisterAudioSource()
    {
        if (!isRegistered || audioSource == null || AudioSettingsManager.Instance == null)
        {
            isRegistered = false;
            return;
        }

        AudioSettingsManager.Instance.UnregisterSource(audioSource);
        isRegistered = false;
    }
}
