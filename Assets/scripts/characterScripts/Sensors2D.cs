using UnityEngine;

[DisallowMultipleComponent]
public class Sensors2D : MonoBehaviour
{
    [Header("Transforms")]
    public Transform feet;
    public Transform head;
    public Transform wallL;
    public Transform wallR;

    [Header("Layer Masks")]
    public LayerMask solidMask;       // Ground | MovingPlatform (solid)
    public LayerMask wallMask;        // genelde Ground ile aynı
    public LayerMask ladderMask;      // Ladder (trigger)
    public LayerMask interactMask;    // Interactable (trigger)
    public LayerMask climbableMask;   // Climbable (trigger)
    public LayerMask jumpableMask;    // Jumpable (trigger)
    public LayerMask ceilingMask;     // Tavan için (Ground)

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
    public bool onLadder { get; private set; }
    public Interactable BestInteractable { get; private set; }
    public bool onClimbableLeft  { get; private set; }
    public bool onClimbableRight { get; private set; }
    public bool onClimbableAny   => onClimbableLeft || onClimbableRight;
    public bool onJumpableLeft   { get; private set; }
    public bool onJumpableRight  { get; private set; }
    public bool onJumpableAny    => onJumpableLeft || onJumpableRight;
    public Collider2D groundCollider { get; private set; }

    bool prevGrounded;
    readonly Collider2D[] interactOverlap = new Collider2D[16];

    void Awake()
    {
        // Boş bıraktıysan otomatik: solid
        if (ceilingMask.value == 0)
            ceilingMask = solidMask;
    }

    public void Sample()
    {
        // Ground (solid)
        var groundHit = Physics2D.OverlapCircle(feet.position, feetRadius, solidMask);
        isGrounded = groundHit != null;
        groundCollider = groundHit;
        justLanded = !prevGrounded && isGrounded;
        prevGrounded = isGrounded;

        // Tavan (ceiling) kontrolü
        if (head)
            headBlocked = Physics2D.OverlapCircle((Vector2)head.position, headRadius, ceilingMask);
        else
            headBlocked = false;

        // Walls (solid ray)
        wallLeft  = Physics2D.Raycast(wallL.position, Vector2.left,  wallDist, wallMask);
        wallRight = Physics2D.Raycast(wallR.position, Vector2.right, wallDist, wallMask);

        // Ladder (trigger)
        onLadder = Physics2D.OverlapCircle(transform.position, 0.16f, ladderMask);

        // Climbable (trigger) — sol/sağ duvar köşelerinde küçük kapsülle kontrol
        onClimbableLeft  = Physics2D.OverlapCircle(wallL.position, 0.12f, climbableMask);
        onClimbableRight = Physics2D.OverlapCircle(wallR.position, 0.12f, climbableMask);

        // Jumpable (trigger) — sol/sağ duvar köşelerinde küçük kapsülle kontrol
        onJumpableLeft   = Physics2D.OverlapCircle(wallL.position, 0.12f, jumpableMask);
        onJumpableRight  = Physics2D.OverlapCircle(wallR.position, 0.12f, jumpableMask);


        // Interactable (trigger)
        FindBestInteractable();
    }

    void FindBestInteractable()
    {
        BestInteractable = null;
        
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(interactMask);
        filter.useTriggers = true; // NonAlloc version implicitly used triggers
        
        int count = Physics2D.OverlapCircle(transform.position, interactRad, filter, interactOverlap);

        float bestDist = float.MaxValue;
        int bestPriority = int.MinValue;

        for (int i = 0; i < count; i++)
        {
            var c = interactOverlap[i];
            var interactable = c.GetComponent<Interactable>() ?? c.GetComponentInParent<Interactable>();
            if (!interactable) continue;

            // Note: We don't check AllowsActor here, controller does that.
            float dist = Vector2.Distance(transform.position, c.transform.position);
            if (dist > interactable.range) continue;

            if (interactable.priority > bestPriority || (interactable.priority == bestPriority && dist < bestDist))
            {
                bestPriority = interactable.priority;
                bestDist = dist;
                BestInteractable = interactable;
            }
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


        // Ladder check
        Gizmos.color = onLadder ? Color.blue : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.16f);

        // Interactable check
        Gizmos.color = BestInteractable ? Color.magenta : Color.white;
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


        // Ladder check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.16f);

        // Interactable check
        Gizmos.color = new Color(0.5f, 0f, 0.5f); // Purple
        Gizmos.DrawWireSphere(transform.position, interactRad);
    }
}
