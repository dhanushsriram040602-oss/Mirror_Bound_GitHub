using System.Collections;
using UnityEngine;

/// <summary>
/// Code-driven procedural animator for all World 2 enemies.
/// Drives Transform scale, position bob, and SpriteRenderer color glow
/// entirely in code — no animation clips required for single-frame pixel art.
///
/// Each enemy state maps to a distinct motion profile (amplitude, frequency,
/// squash, color pulse) that is blended smoothly on state transition.
/// This replaces .anim clip files and works cleanly with the Animator FSM
/// whose transitions remain parameter-driven by the enemy scripts.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyAnimator : MonoBehaviour
{
    // ── Profile Config ───────────────────────────────────────────────────

    public enum EnemyType
    {
        VoidStalker,
        PhaseWraith,
        CeilingCrawler,
        RiftPulse,
        EchoShade
    }

    [Header("Enemy Type")]
    public EnemyType enemyType = EnemyType.VoidStalker;

    [Header("Patrol Bob")]
    public float patrolBobAmplitude  = 0.055f;
    public float patrolBobFrequency  = 2.4f;

    [Header("Charge Squash")]
    public float chargeSquashX       = 1.3f;
    public float chargeSquashY       = 0.78f;

    [Header("Ghost Fade")]
    [Range(0f, 1f)]
    public float ghostAlphaTarget    = 0.045f;
    public float ghostFadeDuration   = 0.25f;

    [Header("Glow Pulse")]
    public float glowPulseAmplitude  = 0.22f;
    public float glowPulseFrequency  = 1.8f;

    [Header("Phase Wraith — Float")]
    public float wraithTiltAmplitude = 8f;
    public float wraithTiltFrequency = 0.9f;

    [Header("Rift Pulse — Spin")]
    public float pulseIdleRotSpeed   = 18f;
    public float pulseFireScalePeak  = 1.35f;
    public float pulseFireDuration   = 0.18f;

    [Header("Echo Shade — Glitch")]
    public float echoGlitchInterval  = 0.6f;
    public float echoGlitchMagnitude = 0.04f;

    // ── Internal State ───────────────────────────────────────────────────

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private Color   baseColor;

    // Animator state tracking (read from Animator hash each frame)
    private Animator anim;
    private static readonly int HashIsActive   = Animator.StringToHash("isActive");
    private static readonly int HashIsCharging = Animator.StringToHash("isCharging");
    private static readonly int HashIsFiring   = Animator.StringToHash("isFiring");
    private static readonly int HashIsPulsing  = Animator.StringToHash("isPulsing");

    // Ghost crossfade
    private Coroutine ghostRoutine;
    private bool wasActive = true;

    // Rift Pulse fire flash
    private bool  isFiringFlash;
    private float fireFlashTimer;

    // Echo Shade glitch timer
    private float echoGlitchTimer;

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        sr         = GetComponent<SpriteRenderer>();
        anim       = GetComponent<Animator>();
        baseScale  = transform.localScale;
        baseLocalPos = transform.localPosition;
        baseColor  = sr.color;
    }

    private void Update()
    {
        float t = Time.time;

        switch (enemyType)
        {
            case EnemyType.VoidStalker:
                AnimateVoidStalker(t);
                break;

            case EnemyType.PhaseWraith:
                AnimatePhaseWraith(t);
                break;

            case EnemyType.CeilingCrawler:
                AnimateCeilingCrawler(t);
                break;

            case EnemyType.RiftPulse:
                AnimateRiftPulse(t);
                break;

            case EnemyType.EchoShade:
                AnimateEchoShade(t);
                break;
        }
    }

    // ── Public API (called by enemy scripts) ─────────────────────────────

    /// <summary>Triggers the charge squash-and-stretch animation.</summary>
    public void TriggerCharge()
    {
        StopAllCoroutines();
        StartCoroutine(ChargeSquashRoutine());
    }

    /// <summary>Triggers the fire-drop recoil animation on CeilingCrawler.</summary>
    public void TriggerFireDrop()
    {
        StopAllCoroutines();
        StartCoroutine(FireDropRecoilRoutine());
    }

    /// <summary>Triggers the pulse fire scale flash on RiftPulse.</summary>
    public void TriggerPulseFlash()
    {
        isFiringFlash = true;
        fireFlashTimer = pulseFireDuration;
    }

    /// <summary>Crossfades the sprite to ghost state (called by EnemyBase).</summary>
    public void SetGhost(bool isGhost)
    {
        if (ghostRoutine != null) StopCoroutine(ghostRoutine);
        ghostRoutine = StartCoroutine(GhostFadeRoutine(isGhost));
    }

    // ── Per-Enemy Animation ──────────────────────────────────────────────

    private void AnimateVoidStalker(float t)
    {
        bool charging = HasBoolParam(HashIsCharging);

        if (charging)
        {
            // During charge: flatten horizontally, stretch vertically — held by coroutine
            return;
        }

        // Patrol walk bob — gentle side-to-side lean + vertical bob
        float bob    = Mathf.Sin(t * patrolBobFrequency * Mathf.PI * 2f) * patrolBobAmplitude;
        float scaleX = baseScale.x + Mathf.Abs(bob) * 0.08f;
        float scaleY = baseScale.y - Mathf.Abs(bob) * 0.06f;

        transform.localScale = new Vector3(
            scaleX * (sr.flipX ? -1f : 1f),
            scaleY,
            baseScale.z
        );

        // Glow pulse — eyes glow brightens subtly on every step
        PulseGlow(t, glowPulseAmplitude * 0.5f, glowPulseFrequency * 2f);
    }

    private void AnimatePhaseWraith(float t)
    {
        // Sinusoidal Z rotation — the wraith tilts as it floats
        float tilt = Mathf.Sin(t * wraithTiltFrequency * Mathf.PI * 2f) * wraithTiltAmplitude;
        transform.localEulerAngles = new Vector3(0f, 0f, tilt);

        // Scale breathe — alive, pulsing feel
        float breathe = 1f + Mathf.Sin(t * glowPulseFrequency * Mathf.PI * 2f) * 0.04f;
        transform.localScale = new Vector3(
            baseScale.x * breathe * (sr.flipX ? -1f : 1f),
            baseScale.y * breathe,
            baseScale.z
        );

        // Glow pulse — alternates orange and cyan in sync with phase timer
        PulseGlow(t, glowPulseAmplitude, glowPulseFrequency);
    }

    private void AnimateCeilingCrawler(float t)
    {
        // Root transform is NOT touched here — the Animator drives child bones
        // (Body, LegFL/FR/ML/MR/BL/BR, EyeGlow) via AnimationClips.
        // EnemyAnimator only handles the glow pulse on the root SpriteRenderer,
        // which is a separate renderer from the child bone renderers.
        PulseGlow(t, glowPulseAmplitude * 0.6f, patrolBobFrequency);
    }

    private void AnimateRiftPulse(float t)
    {
        // Continuous slow spin — turret rotates to signal it is alive
        transform.localEulerAngles = new Vector3(0f, 0f, t * pulseIdleRotSpeed);

        // Fire flash — scale pop when firing
        if (isFiringFlash)
        {
            fireFlashTimer -= Time.deltaTime;
            float progress = 1f - (fireFlashTimer / pulseFireDuration);
            float scale    = Mathf.Lerp(pulseFireScalePeak, 1f, progress);
            transform.localScale = new Vector3(scale, scale, 1f);

            if (fireFlashTimer <= 0f)
            {
                isFiringFlash = false;
                transform.localScale = baseScale;
            }
            return;
        }

        // Idle breathe — slow pulsing glow
        float breathe = 1f + Mathf.Sin(t * glowPulseFrequency * Mathf.PI * 2f) * 0.05f;
        transform.localScale = new Vector3(breathe, breathe, 1f);

        PulseGlow(t, glowPulseAmplitude, glowPulseFrequency);
    }

    private void AnimateEchoShade(float t)
    {
        // Position glitch — occasional snap offset for corrupted-data feel
        echoGlitchTimer -= Time.deltaTime;
        if (echoGlitchTimer <= 0f)
        {
            echoGlitchTimer = echoGlitchInterval + Random.Range(-0.15f, 0.15f);

            float glitchX = Random.Range(-echoGlitchMagnitude, echoGlitchMagnitude);
            float glitchY = Random.Range(-echoGlitchMagnitude, echoGlitchMagnitude);
            transform.localPosition = new Vector3(
                baseLocalPos.x + glitchX,
                baseLocalPos.y + glitchY,
                baseLocalPos.z
            );
        }

        // Scale flicker — unstable shimmering
        float flicker = 1f + Mathf.Sin(t * 8f) * 0.015f;
        transform.localScale = new Vector3(
            baseScale.x * flicker,
            baseScale.y * flicker,
            baseScale.z
        );

        // Glow pulse — slow, eerie cyan breathe
        PulseGlow(t, glowPulseAmplitude * 0.7f, glowPulseFrequency * 0.6f);
    }

    // ── Coroutines ───────────────────────────────────────────────────────

    private IEnumerator ChargeSquashRoutine()
    {
        const float rampDuration = 0.06f;
        const float holdDuration = 0.55f;
        const float recovDuration = 0.12f;

        // Squash on charge launch
        float elapsed = 0f;
        while (elapsed < rampDuration)
        {
            float p = elapsed / rampDuration;
            float sx = Mathf.Lerp(baseScale.x, chargeSquashX, p);
            float sy = Mathf.Lerp(baseScale.y, chargeSquashY, p);
            transform.localScale = new Vector3(sx * (sr.flipX ? -1f : 1f), sy, baseScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        // Recover
        elapsed = 0f;
        Vector3 squashedScale = transform.localScale;
        while (elapsed < recovDuration)
        {
            float p = elapsed / recovDuration;
            float sx = Mathf.Lerp(squashedScale.x, baseScale.x * (sr.flipX ? -1f : 1f), p);
            float sy = Mathf.Lerp(squashedScale.y, baseScale.y, p);
            transform.localScale = new Vector3(sx, sy, baseScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
    }

    private IEnumerator FireDropRecoilRoutine()
    {
        const float recoilDuration = 0.08f;
        const float recovDuration  = 0.12f;
        const float recoilOffset   = 0.12f;

        // Recoil upward (crawler pushes against ceiling)
        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            float p = elapsed / recoilDuration;
            float y = Mathf.Lerp(0f, recoilOffset, p);
            transform.localPosition = new Vector3(baseLocalPos.x, baseLocalPos.y + y, baseLocalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Recover
        elapsed = 0f;
        while (elapsed < recovDuration)
        {
            float p = elapsed / recovDuration;
            float y = Mathf.Lerp(recoilOffset, 0f, p);
            transform.localPosition = new Vector3(baseLocalPos.x, baseLocalPos.y + y, baseLocalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = baseLocalPos;
    }

    private IEnumerator GhostFadeRoutine(bool toGhost)
    {
        float startAlpha = sr.color.a;
        float endAlpha   = toGhost ? ghostAlphaTarget : 1f;
        float elapsed    = 0f;

        while (elapsed < ghostFadeDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / ghostFadeDuration;
            Color c = sr.color;
            c.a = Mathf.Lerp(startAlpha, endAlpha, p);
            sr.color = c;
            yield return null;
        }

        Color final = sr.color;
        final.a = endAlpha;
        sr.color = final;
        ghostRoutine = null;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void PulseGlow(float t, float amplitude, float frequency)
    {
        if (sr == null) return;
        float pulse = 1f + Mathf.Sin(t * frequency * Mathf.PI * 2f) * amplitude;
        Color c = baseColor;
        c.r = Mathf.Clamp(c.r * pulse, 0f, 8f);
        c.g = Mathf.Clamp(c.g * pulse, 0f, 8f);
        c.b = Mathf.Clamp(c.b * pulse, 0f, 8f);
        sr.color = c;
    }

    private bool HasBoolParam(int hash)
    {
        if (anim == null) return false;
        foreach (var param in anim.parameters)
        {
            if (param.nameHash == hash && param.type == AnimatorControllerParameterType.Bool)
                return anim.GetBool(hash);
        }
        return false;
    }
}
