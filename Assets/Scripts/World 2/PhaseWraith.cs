using System.Collections;
using UnityEngine;

/// <summary>
/// Floating enemy that independently cycles between realities on its own timer,
/// decoupled from the player's shifts. In its active reality it is fully lethal;
/// in the inactive reality it is ghosted. Moves in a sinusoidal float path.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PhaseWraith : EnemyBase
{
    // ── Float Path ───────────────────────────────────────────────────────

    [Header("Float Path")]
    public float floatAmplitude  = 1.2f;
    public float floatFrequency  = 0.8f;
    public float horizontalSpeed = 1.5f;
    public float patrolDistance  = 4f;

    // ── Auto Phase ───────────────────────────────────────────────────────

    [Header("Auto Phase")]
    public float  phaseInterval    = 3f;
    public string phaseParticlePool = "RealityShift";

    // ── Private State ────────────────────────────────────────────────────

    private Rigidbody2D rb;
    private Vector3     startPos;
    private int         patrolDirection = 1;
    private Coroutine   phaseTimerRoutine;

    // The wraith's own internal reality — starts in RealityA.
    private DualRealityObject.ObjectType internalReality = DualRealityObject.ObjectType.RealityA;

    // ── EnemyBase Hooks ──────────────────────────────────────────────────

    protected override void OnAwake()
    {
        rb       = GetComponent<Rigidbody2D>();
        startPos = transform.position;

        // Starts in Reality A
        enemyReality   = DualRealityObject.ObjectType.RealityA;
        internalReality = DualRealityObject.ObjectType.RealityA;

        colorA = new Color(1f, 0.5f, 0f) * 2f;
        colorB = new Color(0f, 0.9f, 1f) * 2f;

        if (rb != null)
        {
            rb.bodyType    = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    protected override void OnStart()
    {
        phaseTimerRoutine = StartCoroutine(PhaseTimerRoutine());
    }

    protected override void OnRealityActivated()
    {
        // Tint to match the reality we are active in
        if (spriteRenderer != null)
        {
            spriteRenderer.color = internalReality == DualRealityObject.ObjectType.RealityA
                ? new Color(colorA.r, colorA.g, colorA.b, 1f)
                : new Color(colorB.r, colorB.g, colorB.b, 1f);
        }
    }

    protected override void OnRealityDeactivated() { }

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void Update()
    {
        RunFloatPatrol();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    // ── Float Patrol ─────────────────────────────────────────────────────

    private void RunFloatPatrol()
    {
        float offset = transform.position.x - startPos.x;

        if (offset > patrolDistance && patrolDirection > 0)
        {
            patrolDirection = -1;
            if (spriteRenderer != null) spriteRenderer.flipX = true;
        }
        else if (offset < -patrolDistance && patrolDirection < 0)
        {
            patrolDirection = 1;
            if (spriteRenderer != null) spriteRenderer.flipX = false;
        }

        float newX = transform.position.x + patrolDirection * horizontalSpeed * Time.deltaTime;
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        transform.position = new Vector3(newX, newY, transform.position.z);
    }

    // ── Auto Phase Coroutine ─────────────────────────────────────────────

    private IEnumerator PhaseTimerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(phaseInterval);

            // Flip internal reality
            internalReality = internalReality == DualRealityObject.ObjectType.RealityA
                ? DualRealityObject.ObjectType.RealityB
                : DualRealityObject.ObjectType.RealityA;

            // Mutate the base enemyReality so HandleRealityChange evaluates correctly
            enemyReality = internalReality;
            RefreshBaseColor();

            // Re-evaluate active state against the current player reality
            ForceRealityEvaluation();

            // Spawn phase-flip particle
            SpawnPhaseParticle();
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SpawnPhaseParticle()
    {
        if (ParticlePoolManager.Instance == null) return;

        ParticleSystem ps = ParticlePoolManager.Instance.SpawnParticle(phaseParticlePool, transform.position);
        if (ps == null) return;

        var main = ps.main;
        main.startColor = internalReality == DualRealityObject.ObjectType.RealityA
            ? new Color(1f, 0.5f, 0f)
            : new Color(0f, 0.9f, 1f);
    }
}
