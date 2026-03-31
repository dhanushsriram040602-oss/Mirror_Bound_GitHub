using System.Collections;
using UnityEngine;

/// <summary>
/// Reality-A-only ground patrol enemy.
/// Charges toward the player when Reality A is entered or when the player
/// wanders within detection radius. Ghosted and harmless in Reality B.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class VoidStalker : EnemyBase
{
    // ── Patrol ───────────────────────────────────────────────────────────

    [Header("Patrol")]
    public float patrolSpeed    = 2f;
    public float patrolDistance = 3f;

    // ── Charge ───────────────────────────────────────────────────────────

    [Header("Charge")]
    public float chargeSpeed     = 14f;
    public float chargeDuration  = 0.6f;
    public float detectionRadius = 5f;

    // ── Audio ────────────────────────────────────────────────────────────

    [Header("Audio")]
    public float chargePitch = 1.3f;

    // ── Private State ────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private Vector3 startPos;
    private int patrolDirection = 1;
    private bool isCharging;
    private Coroutine chargeRoutine;
    private Transform playerTransform;

    // ── EnemyBase Hooks ──────────────────────────────────────────────────

    protected override void OnAwake()
    {
        enemyReality = DualRealityObject.ObjectType.RealityA;
        colorA       = new Color(1f, 0.5f, 0f) * 2f;

        rb       = GetComponent<Rigidbody2D>();
        startPos = transform.position;
    }

    protected override void OnStart()
    {
        FindPlayer();
    }

    protected override void OnRealityActivated()
    {
        // Snap-aggro: charge immediately if player is nearby when we materialise
        if (playerTransform != null &&
            Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius * 1.5f)
        {
            StartCharge();
        }
    }

    protected override void OnRealityDeactivated()
    {
        StopCurrentCharge();
        // Clear velocity before EnemyBase switches the body to Kinematic.
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void Update()
    {
        if (!isActiveInCurrentReality) return;

        if (isCharging) return;

        RunPatrol();

        if (playerTransform != null &&
            Vector2.Distance(transform.position, playerTransform.position) <= detectionRadius)
        {
            StartCharge();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider);
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
        if (spriteRenderer != null)
            spriteRenderer.flipX = patrolDirection < 0;
    }

    // ── Charge ───────────────────────────────────────────────────────────

    private void StartCharge()
    {
        if (isCharging) return;

        StopCurrentCharge();
        chargeRoutine = StartCoroutine(ChargeRoutine());
    }

    private void StopCurrentCharge()
    {
        if (chargeRoutine != null)
        {
            StopCoroutine(chargeRoutine);
            chargeRoutine = null;
        }

        isCharging = false;
    }

    private IEnumerator ChargeRoutine()
    {
        isCharging = true;

        float chargeTargetX = playerTransform != null ? playerTransform.position.x : transform.position.x;

        // Face the charge direction
        bool chargingRight = chargeTargetX > transform.position.x;
        if (spriteRenderer != null) spriteRenderer.flipX = !chargingRight;

        // Animator parameter
        Animator chargeAnim = GetComponent<Animator>();
        if (chargeAnim != null) chargeAnim.SetBool("isCharging", true);

        // Procedural squash-and-stretch
        EnemyAnimator ea = GetComponent<EnemyAnimator>();
        ea?.TriggerCharge();

        // Audio
        PlayChargeSfx();

        float chargeDir = chargingRight ? 1f : -1f;
        float elapsed   = 0f;

        while (elapsed < chargeDuration)
        {
            if (rb != null)
                rb.linearVelocity = new Vector2(chargeDir * chargeSpeed, rb.linearVelocity.y);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Clear animator parameter
        if (chargeAnim != null) chargeAnim.SetBool("isCharging", false);

        isCharging = false;
        chargeRoutine = null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void FindPlayer()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) playerTransform = pc.transform;
    }

    private void PlayChargeSfx()
    {
        if (AudioManager.Instance == null) return;

        // Temporarily adjust pitch for a sharper charge sound, then restore
        AudioSource sfx = AudioManager.Instance.GetComponent<AudioSource>();
        if (sfx == null) return;

        float savedPitch = sfx.pitch;
        sfx.pitch = chargePitch;
        AudioManager.Instance.PlaySwitch();
        sfx.pitch = savedPitch;
    }
}
