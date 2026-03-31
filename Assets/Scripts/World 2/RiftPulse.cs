using System.Collections;
using UnityEngine;

/// <summary>
/// Stationary turret that fires radial RiftRing projectiles at timed intervals.
/// Orange rings are lethal in Reality A; cyan rings are lethal in Reality B.
/// The turret body tints to match the current reality, making its threat clear.
/// </summary>
public class RiftPulse : EnemyBase
{
    // ── Pulse Settings ───────────────────────────────────────────────────

    [Header("Pulse Settings")]
    public float      pulseInterval   = 2f;
    public int        ringCount       = 4;
    public float      ringSpeed       = 3.5f;
    public float      ringLifetime    = 2.5f;
    public GameObject riftRingPrefab;

    [Header("Spread")]
    [Tooltip("false = cardinal 4-way, true = 8-directional diagonal spread")]
    public bool diagonalSpread = false;

    public string pulseParticlePool = "RealityShift";

    // ── Private State ────────────────────────────────────────────────────

    private Coroutine pulseRoutine;

    // ── EnemyBase Hooks ──────────────────────────────────────────────────

    protected override void OnAwake()
    {
        // RiftPulse is visible in both realities — body tint changes per reality
        enemyReality = DualRealityObject.ObjectType.Both;
        colorBoth    = colorA; // starts orange (A)
    }

    protected override void OnStart()
    {
        pulseRoutine = StartCoroutine(PulseRoutine());
    }

    protected override void OnRealityActivated()
    {
        // Body tint changes to match the current lethal reality
        UpdateBodyTint();
    }

    protected override void OnRealityDeactivated()
    {
        // Never actually deactivated (ObjectType.Both) — no-op
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    // ── Pulse Coroutine ──────────────────────────────────────────────────

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(pulseInterval);
            FirePulse();
        }
    }

    private void FirePulse()
    {
        if (riftRingPrefab == null) return;

        int   totalRings  = diagonalSpread ? ringCount * 2 : ringCount;
        float angleStep   = 360f / totalRings;
        float startAngle  = diagonalSpread ? 45f : 0f;

        for (int i = 0; i < totalRings; i++)
        {
            float  angle = startAngle + i * angleStep;
            float  rad   = angle * Mathf.Deg2Rad;
            Vector2 dir  = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject ringObj = Instantiate(riftRingPrefab, transform.position, Quaternion.identity);
            RiftRing ring      = ringObj.GetComponent<RiftRing>();

            if (ring != null)
            {
                ring.lethalReality = currentReality;
                ring.moveDirection = dir;
                ring.moveSpeed     = ringSpeed;
                ring.lifetime      = ringLifetime;
                ring.ringColor     = currentReality == RealityManager.Reality.A ? colorA : colorB;
            }
        }

        // Animator trigger + procedural flash
        if (TryGetComponent<Animator>(out var anim)) anim.SetTrigger("isPulsing");
        EnemyAnimator ea = GetComponent<EnemyAnimator>();
        ea?.TriggerPulseFlash();

        // Particle cue at turret origin
        if (ParticlePoolManager.Instance != null)
        {
            ParticleSystem ps = ParticlePoolManager.Instance.SpawnParticle(pulseParticlePool, transform.position);
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = currentReality == RealityManager.Reality.A
                    ? new Color(1f, 0.5f, 0f)
                    : new Color(0f, 0.9f, 1f);
            }
        }
    }

    // ── Visuals ──────────────────────────────────────────────────────────

    private void UpdateBodyTint()
    {
        if (spriteRenderer == null) return;
        Color c = currentReality == RealityManager.Reality.A ? colorA : colorB;
        c.a = 1f;
        spriteRenderer.color = c;
    }
}
