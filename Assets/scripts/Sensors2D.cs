using UnityEngine;

[DisallowMultipleComponent]
public class Sensors2D : MonoBehaviour
{
    [Header("Transforms")]
    public Transform feet;
    public Transform head;
    public Transform wallL;
    public Transform wallR;
    public Transform ledgeProbe; // göğüs/baş hizası, ileri uca yakın
    
    [Header("Ledge Probe")]
    public float ledgeProbeDistance = 0.3f; // ledge probe'un ne kadar ileriye bakacağı

    [Header("Layer Masks")]
    public LayerMask solidMask;       // Ground | OneWay | MovingPlatform (solid)
    public LayerMask oneWayMask;      // sadece OneWay
    public LayerMask wallMask;        // genelde Ground ile aynı
    public LayerMask ladderMask;      // Ladder (trigger)
    public LayerMask ledgeMask;       // Ledge (trigger)
    public LayerMask interactMask;    // Interactable (trigger)
    public LayerMask climbableMask;   // Climbable (trigger)

    [Header("Sizes / Distances")]
    public float feetRadius  = 0.12f;
    public float headRadius  = 0.12f;
    public float wallDist    = 0.18f;
    public float interactRad = 0.6f;

    // OUTPUTS
    public bool isGrounded { get; private set; }
    public bool justLanded { get; private set; }
    public bool headBlocked { get; private set; }
    public bool wallLeft { get; private set; }
    public bool wallRight { get; private set; }
    public bool onOneWay { get; private set; }
    public bool onLadder { get; private set; }
    public bool isLedge  { get; private set; }
    public LedgeMarker activeLedge { get; private set; }
    public Interactable nearestInteractable { get; private set; }
    public bool onClimbableLeft  { get; private set; }
    public bool onClimbableRight { get; private set; }
    public bool onClimbableAny   => onClimbableLeft || onClimbableRight;

    bool prevGrounded;
    LocomotionMotor2D motor;
    int lastFacingSign = 1; // Cache için

    void Awake()
    {
        motor = GetComponent<LocomotionMotor2D>();
    }

    public void Sample()
    {
        // Ground / OneWay (solid)
        isGrounded = Physics2D.OverlapCircle(feet.position, feetRadius, solidMask);
        onOneWay   = Physics2D.OverlapCircle(feet.position, feetRadius, oneWayMask);
        justLanded = !prevGrounded && isGrounded;
        prevGrounded = isGrounded;

        // Head (solid)
        headBlocked = Physics2D.OverlapCircle(head.position, headRadius, solidMask);

        // Walls (solid ray)
        wallLeft  = Physics2D.Raycast(wallL.position, Vector2.left,  wallDist, wallMask);
        wallRight = Physics2D.Raycast(wallR.position, Vector2.right, wallDist, wallMask);

        // Ladder (trigger)
        onLadder = Physics2D.OverlapCircle(transform.position, 0.16f, ladderMask);

        // Climbable (trigger) — sol/sağ duvar köşelerinde küçük kapsülle kontrol
        onClimbableLeft  = Physics2D.OverlapCircle(wallL.position, 0.12f, climbableMask);
        onClimbableRight = Physics2D.OverlapCircle(wallR.position, 0.12f, climbableMask);

        // Ledge (trigger first, then geometric fallback)
        activeLedge = null;
        isLedge = false;
        
        // LedgeProbe'u sadece facing değiştiğinde konumlandır (optimizasyon)
        if (ledgeProbe && motor && motor.facingSign != lastFacingSign)
        {
            Vector3 probePos = transform.position;
            probePos.x += motor.facingSign * ledgeProbeDistance;
            ledgeProbe.position = probePos;
            lastFacingSign = motor.facingSign;
        }
        
        var hitL = Physics2D.OverlapCircle(ledgeProbe.position, 0.12f, ledgeMask);
        if (hitL)
        {
            activeLedge = hitL.GetComponent<LedgeMarker>();
            if (!activeLedge) activeLedge = hitL.transform.GetComponentInParent<LedgeMarker>();
        }
        if (activeLedge != null)
        {
            isLedge = true;
        }
        else if (!isGrounded)
        {
            // Fallback: üst boş - alt dolu kenar tespiti (dayanıklı olmayabilir)
            bool spaceAtHead = !Physics2D.OverlapCircle(head.position, headRadius, solidMask);
            bool groundBelow = Physics2D.Raycast(ledgeProbe.position, Vector2.down, 0.4f, solidMask);
            isLedge = spaceAtHead && groundBelow;
        }

        // Interactable (trigger)
        nearestInteractable = null;
        var colliders = Physics2D.OverlapCircleAll(transform.position, interactRad, interactMask);
        float best = float.MaxValue;
        foreach (var c in colliders)
        {
            var i = c.GetComponent<Interactable>() ?? c.GetComponentInParent<Interactable>();
            if (!i) continue;
            float d = Vector2.SqrMagnitude((Vector2)c.transform.position - (Vector2)transform.position);
            if (d < best) { best = d; nearestInteractable = i; }
        }
    }

    // Gizmos çizimi - Scene view'da görsel debug
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Feet check (Ground)
        Gizmos.color = isGrounded ? Color.green : Color.red;
        if (feet) Gizmos.DrawWireSphere(feet.position, feetRadius);

        // Head check (Ceiling)
        Gizmos.color = headBlocked ? Color.red : Color.green;
        if (head) Gizmos.DrawWireSphere(head.position, headRadius);

        // Wall checks (Left/Right)
        Gizmos.color = wallLeft ? Color.green : Color.red;
        if (wallL) Gizmos.DrawLine(wallL.position, wallL.position + Vector3.left * wallDist);

        Gizmos.color = wallRight ? Color.green : Color.red;
        if (wallR) Gizmos.DrawLine(wallR.position, wallR.position + Vector3.right * wallDist);

        // Ledge probe
        Gizmos.color = isLedge ? Color.yellow : Color.cyan;
        if (ledgeProbe) Gizmos.DrawWireSphere(ledgeProbe.position, 0.12f);

        // Ladder check
        Gizmos.color = onLadder ? Color.blue : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.16f);

        // Interactable check
        Gizmos.color = nearestInteractable ? Color.magenta : Color.white;
        Gizmos.DrawWireSphere(transform.position, interactRad);
    }

    // Her zaman gizmos göster (Editor'da)
    void OnDrawGizmosSelected()
    {
        // Feet check
        Gizmos.color = Color.yellow;
        if (feet) Gizmos.DrawWireSphere(feet.position, feetRadius);

        // Head check
        Gizmos.color = Color.blue;
        if (head) Gizmos.DrawWireSphere(head.position, headRadius);

        // Wall checks
        Gizmos.color = Color.cyan;
        if (wallL) Gizmos.DrawLine(wallL.position, wallL.position + Vector3.left * wallDist);
        if (wallR) Gizmos.DrawLine(wallR.position, wallR.position + Vector3.right * wallDist);

        // Ledge probe
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
        if (ledgeProbe) Gizmos.DrawWireSphere(ledgeProbe.position, 0.12f);

        // Ladder check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.16f);

        // Interactable check
        Gizmos.color = new Color(0.5f, 0f, 0.5f); // Purple
        Gizmos.DrawWireSphere(transform.position, interactRad);
    }
}
