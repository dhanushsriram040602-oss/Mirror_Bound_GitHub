using System.Collections;
using UnityEngine;

/// <summary>
/// Gravity Inversion Zone — flips the player's gravity while they are inside the trigger.
/// Uses a ref-count so nested or adjacent zones never double-flip or leave gravity stuck.
/// Rotates modelTransform (the visual child) 180° on X so the player appears upside-down.
/// HandleFlip() in PlayerController owns the Y rotation for left/right facing, so we only
/// touch the X axis here — the two axes are completely independent.
/// Audio: plays the jump clip reversed (same jump clip at pitch -1) on both entry and exit.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class GravityZone : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float flippedGravityScale = -1f;

    [Header("Visual Transition")]
    public float flipDuration = 0.15f;

    [Header("Audio")]
    [Range(0f, 2f)]
    public float flipPitch = -1f;

    [Header("Particles")]
    public string ambientParticlePool = "GravityZone";

    // Track how many gravity zones the player is inside (handles overlapping zones).
    private static int gravityZoneCount = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        gravityZoneCount++;

        if (gravityZoneCount == 1)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyFlip(player, flippedGravityScale, flipDuration);
                PlayFlipAudio();
                SpawnEntryParticles(other.transform.position);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        gravityZoneCount = Mathf.Max(0, gravityZoneCount - 1);

        if (gravityZoneCount == 0)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                ApplyFlip(player, 1f, flipDuration);
                PlayFlipAudio();
                SpawnEntryParticles(other.transform.position);
            }
        }
    }

    private void OnDisable()
    {
        // Safety: if the zone is destroyed while the player is inside, reset gravity.
        gravityZoneCount = 0;
    }

    /// <summary>
    /// Applies the gravity scale change and starts the visual flip coroutine on the player.
    /// Uses a coroutine started on the player so it survives if this zone is destroyed.
    /// </summary>
    private void ApplyFlip(PlayerController player, float targetGravityScale, float duration)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.gravityScale = targetGravityScale;

        // Kill any in-progress flip before starting a new one
        player.StopCoroutine("GravityFlipRoutine");
        player.StartCoroutine(GravityFlipRoutine(player, targetGravityScale, duration));
    }

    private IEnumerator GravityFlipRoutine(PlayerController player, float targetGravityScale, float duration)
    {
        if (player == null) yield break;

        // modelTransform is the visual child; we rotate only the X axis
        Transform model = player.modelTransform;
        if (model == null) yield break;

        float targetXAngle = (targetGravityScale < 0f) ? 180f : 0f;
        Vector3 startEuler = model.localEulerAngles;
        Vector3 endEuler   = new Vector3(targetXAngle, startEuler.y, startEuler.z);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (player == null || model == null) yield break;
            elapsed += Time.deltaTime;
            model.localEulerAngles = Vector3.Lerp(startEuler, endEuler, elapsed / duration);
            yield return null;
        }

        if (model != null)
            model.localEulerAngles = endEuler;
    }

    /// <summary>Plays the jump clip at negative pitch for the "reversed jump" gravity-flip sound.</summary>
    private void PlayFlipAudio()
    {
        if (AudioManager.Instance == null) return;

        AudioSource sfxSource = AudioManager.Instance.GetComponent<AudioSource>();
        if (sfxSource == null) return;

        AudioClip clip = AudioManager.Instance.jumpClip;
        if (clip == null) return;

        float savedPitch = sfxSource.pitch;
        sfxSource.pitch = flipPitch;
        sfxSource.PlayOneShot(clip, AudioManager.Instance.masterVolume);
        sfxSource.pitch = savedPitch;
    }

    /// <summary>Spawns a brief burst from the GravityZone pool at the player's position on enter/exit.</summary>
    private void SpawnEntryParticles(Vector3 position)
    {
        if (ParticlePoolManager.Instance == null) return;

        ParticleSystem ps = ParticlePoolManager.Instance.SpawnParticle(ambientParticlePool, position);

        if (ps != null)
        {
            var main = ps.main;
            main.startColor = new Color(0.6f, 0.2f, 1f, 1f);
        }
    }
}
