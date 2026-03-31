using UnityEngine;

/// <summary>
/// Radial projectile fired by RiftPulse.
/// Moves outward in a straight line (kinematic, gravity = 0).
/// Its collider is only active when the player's current reality matches lethalReality,
/// forcing the player to shift in order to dodge.
/// Drives the RiftRing Animator FSM: Idle → Charge → Fire  |  any → Ghost.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RiftRing : MonoBehaviour
{
    public RealityManager.Reality lethalReality = RealityManager.Reality.A;
    public Vector2 moveDirection = Vector2.right;
    public float   moveSpeed     = 3.5f;
    public float   lifetime      = 2.5f;

    [ColorUsage(true, true)]
    public Color ringColor = Color.white * 2f;

    // How long the Charge animation plays before the ring is released.
    public float chargeWindupDuration = 0.4f;

    private const float GhostAlpha = 0.1f;

    // Animator parameter hashes — avoids string lookups every frame.
    private static readonly int HashIsCharging = Animator.StringToHash("isCharging");
    private static readonly int HashIsFiring   = Animator.StringToHash("isFiring");
    private static readonly int HashIsLethal   = Animator.StringToHash("isLethal");

    private Rigidbody2D    rb;
    private Collider2D     col;
    private SpriteRenderer sr;
    private Animator       anim;

    private bool isFlying;

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        col  = GetComponent<Collider2D>();
        sr   = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        if (col != null) col.isTrigger = true;
        if (sr  != null) sr.color      = ringColor;
    }

    private void Start()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += UpdateLethalState;
            UpdateLethalState(RealityManager.Instance.currentReality);
        }

        // Start the windup, then release the ring after chargeWindupDuration seconds.
        SetAnimatorBool(HashIsCharging, true);
        Invoke(nameof(Release), chargeWindupDuration);

        Destroy(gameObject, lifetime);
    }

    private void OnDestroy()
    {
        if (RealityManager.Instance != null)
            RealityManager.Instance.OnRealityChange -= UpdateLethalState;
    }

    private void FixedUpdate()
    {
        if (!isFlying || rb == null) return;
        rb.linearVelocity = moveDirection.normalized * moveSpeed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        player?.Die();
    }

    /// <summary>
    /// Transitions the ring from Charge → Fire state and begins movement.
    /// Called automatically after chargeWindupDuration; can also be called externally.
    /// </summary>
    public void Release()
    {
        isFlying = true;
        SetAnimatorBool(HashIsCharging, false);
        SetAnimatorTrigger(HashIsFiring);
    }

    private void UpdateLethalState(RealityManager.Reality currentReality)
    {
        bool lethal = currentReality == lethalReality;

        if (col != null) col.enabled = lethal;
        SetAnimatorBool(HashIsLethal, lethal);

        // Only tint the root SpriteRenderer if it is still active (pre-child-hierarchy builds).
        if (sr != null && sr.enabled)
        {
            Color c = lethal ? ringColor : ringColor * 0.5f;
            c.a      = lethal ? 1f : GhostAlpha;
            sr.color = c;
        }
    }

    private void SetAnimatorBool(int hash, bool value)
    {
        if (anim != null) anim.SetBool(hash, value);
    }

    private void SetAnimatorTrigger(int hash)
    {
        if (anim != null) anim.SetTrigger(hash);
    }
}
