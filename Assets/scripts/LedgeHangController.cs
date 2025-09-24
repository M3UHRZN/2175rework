using UnityEngine;

[DisallowMultipleComponent]
public class LedgeHangController : MonoBehaviour
{
    public LedgeConfig ledgeCfg;
    InputAdapter input;
    Sensors2D sensors;
    LocomotionMotor2D motor;
    PlayerStateMachine fsm;
    
    // Akıllı ledge detection için
    Vector2 lastWallClimbPos;
    const float MIN_LEDGE_DISTANCE = 1.0f; // Minimum ledge mesafesi

    void Awake()
    {
        input  = GetComponent<InputAdapter>();
        sensors= GetComponent<Sensors2D>();
        motor  = GetComponent<LocomotionMotor2D>();
        fsm    = GetComponent<PlayerStateMachine>();
    }

    public void Tick(float dt)
    {
        if (sensors.isGrounded) return;
        
        // Grab - akıllı koşullar
        if (sensors.isLedge && fsm.Current != PlayerStateMachine.LocoState.LedgeHang)
        {
            // Wall climb sırasında akıllı ledge detection
            if (fsm.Current == PlayerStateMachine.LocoState.WallClimb)
            {
                Vector2 ledgePos = sensors.activeLedge ? sensors.activeLedge.WorldHangPoint : (Vector2)transform.position;
                
                // 1. Sadece yukarıdaki ledge'leri kabul et
                if (ledgePos.y <= transform.position.y + 0.5f)
                {
                    return; // Aşağıdaki ledge'leri görmezden gel
                }
                
                // 2. Minimum mesafe kontrolü (aynı ledge'e tekrar tutunmayı önle)
                float distance = Vector2.Distance(ledgePos, lastWallClimbPos);
                if (distance < MIN_LEDGE_DISTANCE)
                {
                    return; // Çok yakın ledge'leri görmezden gel
                }
                
                // 3. Wall climb pozisyonunu güncelle
                lastWallClimbPos = transform.position;
            }
            
            Vector2 snap = sensors.activeLedge ? sensors.activeLedge.WorldHangPoint
                                               : (Vector2)transform.position; // fallback
            int face = sensors.activeLedge ? Mathf.Clamp(sensors.activeLedge.preferFacing, -1, 1) : 0;
            motor.RequestLedgeHang(snap, face);
            
            // Sadece daha yüksek öncelikli state'e geçiş yapabilir
            if (fsm.CanTransitionTo(PlayerStateMachine.LocoState.LedgeHang))
            {
                fsm.RequestTransition(PlayerStateMachine.LocoState.LedgeHang, "LedgeHang");
            }
            return;
        }

        if (fsm.Current == PlayerStateMachine.LocoState.LedgeHang)
        {
            // Up or toward edge → climb
            int dir = input.MoveX > 0.2f ? 1 : (input.MoveX < -0.2f ? -1 : 0);
            bool goUp = input.MoveY > 0.2f || dir != 0;

            if (goUp)
            {
                Vector2 shift = sensors.activeLedge ? sensors.activeLedge.climbUpShift
                                                    : (ledgeCfg ? ledgeCfg.defaultClimbShift : new Vector2(0.5f, 0.9f));
                if (dir == 0) dir = 1; // default right
                motor.RequestLedgeClimb(dir, shift);
                fsm.RequestTransition(PlayerStateMachine.LocoState.Idle, "LedgeClimb");
            }
            else if (input.DownHeld)
            {
                // drop
                motor.RequestStopClimbRestoreGravity();
                fsm.RequestTransition(PlayerStateMachine.LocoState.JumpFall, "LedgeDrop");
            }
        }
    }
    
    // Wall climb çıkışında pozisyonu sıfırla
    public void OnWallClimbExit()
    {
        lastWallClimbPos = Vector2.zero;
    }
}