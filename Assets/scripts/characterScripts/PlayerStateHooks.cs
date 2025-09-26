using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStateHooks : MonoBehaviour
{
    [Header("State Çıkış Kontrolü")]
    [Tooltip("State çıkış kontrollerini aktif et")]
    public bool enableStateExitChecks = true;
    
    [Tooltip("State çıkış işlemlerini konsola yazdır")]
    public bool logStateExits = false;
    
    PlayerStateMachine fsm;
    LocomotionMotor2D motor;
    Sensors2D sensors;
    InputAdapter input;
    
    void Awake()
    {
        fsm = GetComponent<PlayerStateMachine>();
        motor = GetComponent<LocomotionMotor2D>();
        sensors = GetComponent<Sensors2D>();
        input = GetComponent<InputAdapter>();
        
        if (fsm != null)
        {
            // State değişikliklerini dinle
            fsm.OnLocoChanged += OnStateChanged;
        }
    }
    
    void OnDestroy()
    {
        if (fsm != null)
        {
            fsm.OnLocoChanged -= OnStateChanged;
        }
    }
    
    /// <summary>
    /// State değişikliği olduğunda çağrılır
    /// </summary>
    private void OnStateChanged(PlayerStateMachine.LocoState oldState, PlayerStateMachine.LocoState newState)
    {
        if (logStateExits)
            Debug.Log($"[PlayerStateHooks] {gameObject.name}: State değişti {oldState} -> {newState}", this);
    }
    
    /// <summary>
    /// State çıkış kontrollerini yap - AbilityController'dan çağrılır
    /// </summary>
    public void CheckStateExits()
    {
        if (!enableStateExitChecks) return;
        
        switch (fsm.Current)
        {
            case PlayerStateMachine.LocoState.WallSlide:
                CheckWallSlideExit();
                break;
                
            case PlayerStateMachine.LocoState.WallClimb:
                CheckWallClimbExit();
                break;
                
            case PlayerStateMachine.LocoState.Climb:
                CheckClimbExit();
                break;
        }
    }
    
    /// <summary>
    /// Wall slide'dan çıkış kontrolü
    /// </summary>
    private void CheckWallSlideExit()
    {
        if (sensors.isGrounded)
        {
            // Yere düştüyse Run veya Idle'a geç
            var targetState = Mathf.Abs(input.MoveX) > 0.05f
                ? PlayerStateMachine.LocoState.Run
                : PlayerStateMachine.LocoState.Idle;
                
            fsm.RequestTransition(targetState, "WallSlide->Grounded");
            
            if (logStateExits)
                Debug.Log($"[PlayerStateHooks] WallSlide -> {targetState} (Grounded)", this);
            return;
        }
        
        if (!sensors.wallLeft && !sensors.wallRight)
        {
            // Duvar yoksa düş
            fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "WallSlide->NoWall");
            
            if (logStateExits)
                Debug.Log("[PlayerStateHooks] WallSlide -> JumpFall (No Wall)", this);
            return;
        }
    }
    
    /// <summary>
    /// Wall climb'dan çıkış kontrolü
    /// </summary>
    private void CheckWallClimbExit()
    {
        if (!(sensors.onClimbableAny && (sensors.wallLeft || sensors.wallRight)))
        {
            // Tırmanılabilir yüzey yoksa çıkış yap
            PlayerStateMachine.LocoState targetState;
            string reason;
            
            if (!sensors.isGrounded && (sensors.wallLeft || sensors.wallRight))
            {
                // Duvar var ama tırmanılabilir değilse WallSlide'a geç
                targetState = PlayerStateMachine.LocoState.WallSlide;
                reason = "WallClimb->Slide (Not Climbable)";
            }
            else
            {
                // Hiç duvar yoksa düş
                targetState = PlayerStateMachine.LocoState.JumpFall;
                reason = "WallClimb->Fall (No Wall)";
            }
            
            fsm.RequestTransition(targetState, reason);
            
            if (logStateExits)
                Debug.Log($"[PlayerStateHooks] WallClimb -> {targetState} ({reason})", this);
        }
    }
    
    /// <summary>
    /// Climb'dan çıkış kontrolü
    /// </summary>
    private void CheckClimbExit()
    {
        if (!sensors.onLadder)
        {
            // Merdiven yoksa düş
            fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "Climb->Fall");
            
            if (logStateExits)
                Debug.Log("[PlayerStateHooks] Climb -> JumpFall (No Ladder)", this);
        }
    }
    
    /// <summary>
    /// State'ten çıkarken temizlik işlemlerini yap - PlayerStateMachine'den çağrılır
    /// </summary>
    public void HandleStateExit(PlayerStateMachine.LocoState exitingState)
    {
        switch (exitingState)
        {
            case PlayerStateMachine.LocoState.WallSlide:
                // Wall slide çıkışında özel bir şey yapmaya gerek yok
                // Motor zaten wall slide'ı otomatik durduruyor
                if (logStateExits)
                    Debug.Log("[PlayerStateHooks] WallSlide exit cleanup", this);
                break;
                
            case PlayerStateMachine.LocoState.WallClimb:
                // Wall climb'ı durdur ve gravity'yi restore et
                if (motor != null)
                {
                    motor.StopWallClimbRestoreGravity();
                }
                
                if (logStateExits)
                    Debug.Log("[PlayerStateHooks] WallClimb exit cleanup - gravity restored", this);
                break;
                
            case PlayerStateMachine.LocoState.Climb:
                // Climb'ı durdur ve gravity'yi restore et
                if (motor != null)
                {
                    motor.RequestStopClimbRestoreGravity();
                }
                
                if (logStateExits)
                    Debug.Log("[PlayerStateHooks] Climb exit cleanup - gravity restored", this);
                break;
                
            case PlayerStateMachine.LocoState.JumpRise:
            case PlayerStateMachine.LocoState.JumpFall:
                // Jump state'lerinden çıkışta özel bir şey yapmaya gerek yok
                // Motor zaten gravity'yi otomatik yönetiyor
                if (logStateExits)
                    Debug.Log($"[PlayerStateHooks] {exitingState} exit cleanup", this);
                break;
                
            case PlayerStateMachine.LocoState.Idle:
            case PlayerStateMachine.LocoState.Run:
                // Ground state'lerden çıkışta özel bir şey yapmaya gerek yok
                if (logStateExits)
                    Debug.Log($"[PlayerStateHooks] {exitingState} exit cleanup", this);
                break;
        }
    }
    
    /// <summary>
    /// Default state resolution - AbilityController'dan çağrılır
    /// </summary>
    public void ResolveDefaultStates()
    {
        if (!enableStateExitChecks) return;
        
        // Sadece düşük öncelikli state'lerde çalışır
        if (fsm.Current == PlayerStateMachine.LocoState.Idle || 
            fsm.Current == PlayerStateMachine.LocoState.Run ||
            fsm.Current == PlayerStateMachine.LocoState.JumpRise ||
            fsm.Current == PlayerStateMachine.LocoState.JumpFall)
        {
            if (sensors.isGrounded)
            {
                // Yerdeyse Run veya Idle'a geç
                var targetState = Mathf.Abs(input.MoveX) > 0.05f
                    ? PlayerStateMachine.LocoState.Run
                    : PlayerStateMachine.LocoState.Idle;
                    
                fsm.RequestTransition(targetState, "Grounded");
                
                if (logStateExits)
                    Debug.Log($"[PlayerStateHooks] Default resolution -> {targetState} (Grounded)", this);
            }
            else
            {
                // Havadaysa Jump state'lerine geç
                var targetState = motor.velocityY > 0f
                    ? PlayerStateMachine.LocoState.JumpRise
                    : PlayerStateMachine.LocoState.JumpFall;
                    
                fsm.RequestTransition(targetState, "Airborne");
                
                if (logStateExits)
                    Debug.Log($"[PlayerStateHooks] Default resolution -> {targetState} (Airborne)", this);
            }
        }
    }
}
