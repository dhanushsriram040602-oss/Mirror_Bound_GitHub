using System.Collections;
using UnityEngine;

/// <summary>
/// Echo Platform — activates for a fixed duration every time the player shifts realities.
/// Does not care which reality is active; it only reacts to the shift event itself.
/// On activation: collider enables, sprite flashes to full opacity, a particle burst spawns,
/// and the spawn audio plays at a lowered pitch.
/// On deactivation: collider disables immediately, then the sprite fades out gradually.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EchoPlatform : MonoBehaviour
{
    [Header("Echo Settings")]
    public float activeDuration = 2f;

    [Header("Visuals")]
    [ColorUsage(true, true)]
    public Color activeColor = new Color(0.4f, 1f, 1f, 1f);
    [Range(0f, 1f)]
    public float inactiveAlpha = 0.04f;
    public float fadeOutDuration = 0.5f;

    [Header("Audio")]
    [Range(0f, 2f)]
    public float spawnPitch = 0.6f;

    [Header("Particles")]
    public string spawnParticlePool = "EchoSpawn";

    private BoxCollider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private Coroutine activeRoutine;

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += OnRealityShifted;
        }

        ApplyInactiveColor();
        platformCollider.enabled = false;
    }

    private void OnDestroy()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= OnRealityShifted;
        }
    }

    /// <summary>Triggered on every reality shift regardless of which reality becomes active.</summary>
    private void OnRealityShifted(RealityManager.Reality _)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        activeRoutine = StartCoroutine(EchoRoutine());
    }

    private IEnumerator EchoRoutine()
    {
        // --- Activate ---
        platformCollider.enabled = true;
        ApplyActiveColor();
        PlaySpawnAudio();
        SpawnParticles();

        // Hold solid for (activeDuration - fadeOutDuration) seconds
        float holdTime = Mathf.Max(0f, activeDuration - fadeOutDuration);
        yield return new WaitForSeconds(holdTime);

        // Disable collider immediately — player must not stand on a fading platform
        platformCollider.enabled = false;

        // --- Fade out gradually ---
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color endColor = startColor;
        endColor.a = inactiveAlpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / fadeOutDuration);
            yield return null;
        }

        ApplyInactiveColor();
        activeRoutine = null;
    }

    private void ApplyActiveColor()
    {
        Color c = activeColor;
        c.a = 1f;
        spriteRenderer.color = c;
    }

    private void ApplyInactiveColor()
    {
        Color c = activeColor;
        c.a = inactiveAlpha;
        spriteRenderer.color = c;
    }

    /// <summary>Plays the reality-switch clip at a lower pitch to give the echo spawn a distinct feel.</summary>
    private void PlaySpawnAudio()
    {
        if (AudioManager.Instance == null) return;

        AudioSource sfxSource = AudioManager.Instance.GetComponent<AudioSource>();
        if (sfxSource == null) return;

        AudioClip clip = AudioManager.Instance.switchClip;
        if (clip == null) return;

        float savedPitch = sfxSource.pitch;
        sfxSource.pitch = spawnPitch;
        sfxSource.PlayOneShot(clip, AudioManager.Instance.masterVolume);
        sfxSource.pitch = savedPitch;
    }

    /// <summary>Spawns a burst particle effect from the named pool at this platform's position.</summary>
    private void SpawnParticles()
    {
        if (ParticlePoolManager.Instance == null) return;

        ParticleSystem ps = ParticlePoolManager.Instance.SpawnParticle(spawnParticlePool, transform.position);

        if (ps != null)
        {
            var main = ps.main;
            main.startColor = activeColor;
        }
    }
}
