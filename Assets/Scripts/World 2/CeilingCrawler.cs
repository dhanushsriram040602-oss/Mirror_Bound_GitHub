using UnityEngine;

/// <summary>
/// Gravity-inverted patrol enemy that walks along ceilings and drops
/// RiftDropProjectile instances at regular intervals. Active in both realities.
/// Uses a Kinematic Rigidbody2D and moves via transform — no gravity needed.
///
/// Drives the CeilingCrawler Animator FSM:
///   Patrol ↔ FireDrop  (isFiring Trigger)
///   Patrol ↔ TurnAround (isTurning Trigger)
///   Any    → Ghost      (isActive Bool = false)
///   Any    → Death      (isDead Trigger)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class CeilingCrawler : EnemyBase
{
    // ── Patrol ───────────────────────────────────────────────────────────

    [Header("Patrol")]
    public float patrolSpeed    = 1.8f;
    public float patrolDistance = 3.5f;

    // ── Drop Attack ──────────────────────────────────────────────────────

    [Header("Drop Attack")]
    public float      fireInterval            = 2.5f;
    public GameObject riftDropProjectilePrefab;
    public float      projectileLifetime      = 3f;
    public string     dropParticlePool        = "RealityShift";

    [Header("Audio")]
    public float dropPitch = 0.7f;

    // ── Animator Parameter Hashes ─────────────────────────────────────────
    private static readonly int HashIsFiring    = Animator.StringToHash("isFiring");
    private static readonly int HashIsTurning   = Animator.StringToHash("isTurning");
    private static readonly int HashIsActive    = Animator.StringToHash("isActive");
    private static readonly int HashIsDead      = Animator.StringToHash("isDead");

    // ── Private State ────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private Animator    anim;
    private Vector3 startPos;
    private int patrolDirection = 1;
    private float fireTimer;

    // ── EnemyBase Hooks ──────────────────────────────────────────────────

    protected override void OnAwake()
    {
        enemyReality = DualRealityObject.ObjectType.Both;
        colorBoth    = Color.white * 2f;

        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        startPos  = transform.position;
        fireTimer = fireInterval;
    }

    protected override void OnStart() { }

    protected override void OnRealityActivated()
    {
        // Transition back out of Ghost state when reality makes us lethal again.
        if (anim != null) anim.SetBool(HashIsActive, true);
    }

    protected override void OnRealityDeactivated()
    {
        // Transition into Ghost state — EnemyBase already sets visual alpha
        // via EnemyAnimator; we also drive the FSM so bone animation stops.
        if (anim != null) anim.SetBool(HashIsActive, false);
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void Update()
    {
        RunPatrol();

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f)
        {
            FireDrop();
            fireTimer = fireInterval;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    // ── Patrol ───────────────────────────────────────────────────────────

    private void RunPatrol()
    {
        float offset = transform.position.x - startPos.x;

        if (offset > patrolDistance && patrolDirection > 0)
            TurnAround();
        else if (offset < -patrolDistance && patrolDirection < 0)
            TurnAround();

        transform.position += Vector3.right * patrolDirection * patrolSpeed * Time.deltaTime;
    }

    private void TurnAround()
    {
        patrolDirection *= -1;
        if (spriteRenderer != null) spriteRenderer.flipX = patrolDirection < 0;

        // Fire the turn-around animation — body weight-shift + leg tuck
        if (anim != null) anim.SetTrigger(HashIsTurning);
    }

    // ── Drop Attack ──────────────────────────────────────────────────────

    private void FireDrop()
    {
        if (riftDropProjectilePrefab == null) return;

        GameObject proj = Instantiate(riftDropProjectilePrefab, transform.position, Quaternion.identity);

        // Override lifetime if projectile has the component
        RiftDropProjectile rdp = proj.GetComponent<RiftDropProjectile>();
        if (rdp != null) rdp.lifetime = projectileLifetime;

        // Bone animation trigger — drives the FireDrop clip
        if (anim != null) anim.SetTrigger(HashIsFiring);

        // Procedural recoil on root (EnemyAnimator) — still runs on root transform,
        // complementing the bone-level body/leg animation on child objects.
        EnemyAnimator ea = GetComponent<EnemyAnimator>();
        ea?.TriggerFireDrop();

        // Optional particle cue
        if (ParticlePoolManager.Instance != null)
            ParticlePoolManager.Instance.SpawnParticle(dropParticlePool, transform.position);

        // Audio at lower pitch for a heavy-drop feel
        PlayDropSfx();
    }

    /// <summary>
    /// Triggers the Death animation and disables patrol/fire logic.
    /// Call this when the enemy should play its death sequence.
    /// </summary>
    public void TriggerDeath()
    {
        enabled = false; // stop Update patrol/fire loop
        if (anim != null) anim.SetTrigger(HashIsDead);
    }

    private void PlayDropSfx()
    {
        if (AudioManager.Instance == null) return;

        AudioSource sfx = AudioManager.Instance.GetComponent<AudioSource>();
        if (sfx == null) return;

        float savedPitch = sfx.pitch;
        sfx.pitch = dropPitch;
        AudioManager.Instance.PlaySwitch();
        sfx.pitch = savedPitch;
    }
}
