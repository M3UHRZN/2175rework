using UnityEngine;

[DisallowMultipleComponent]
public class DebugOverlay : MonoBehaviour
{
    PlayerStateMachine fsm;
    Sensors2D s;
    LocomotionMotor2D m;

    void Awake()
    {
        fsm = GetComponent<PlayerStateMachine>();
        s = GetComponent<Sensors2D>();
        m = GetComponent<LocomotionMotor2D>();
    }

    void OnGUI()
    {
        int y = 10;
        GUI.Label(new Rect(10, y, 480, 22), $"State: {fsm.Current}"); y += 20;
        GUI.Label(new Rect(10, y, 480, 22), $"Grounded:{s.isGrounded} just:{s.justLanded} head:{s.headBlocked}"); y += 20;
        GUI.Label(new Rect(10, y, 480, 22), $"Wall L/R:{s.wallLeft}/{s.wallRight}  Ladder:{s.onLadder}"); y += 20;
        GUI.Label(new Rect(10, y, 480, 22), $"VelX:{m.velocityX:0.00} VelY:{m.velocityY:0.00}"); y += 20;
    }
}
