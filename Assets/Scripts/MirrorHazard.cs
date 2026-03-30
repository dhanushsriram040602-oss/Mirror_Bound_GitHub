using System.Collections;
using UnityEngine;

/// <summary>
/// Mirror Hazard — lethal in one reality, safe (and ghosted) in the other.
/// In its lethal reality: collider is an active trigger, sprite is bright and solid.
/// In its safe reality: collider is disabled, sprite fades to ghost alpha — same
/// visual language as DualRealityObject so the player already knows what it means.
/// Reuses the existing Hazard kill path (TryKill via OnTriggerEnter2D).
/// Audio: plays dieClip at a slightly lower pitch on kill, matching the plan note.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class MirrorHazard : MonoBehaviour
{
    public enum LethalIn { RealityA, RealityB }

    [Header("Mirror Settings")]
    public LethalIn lethalReality = LethalIn.RealityB;

    [Header("Visuals")]
    [ColorUsage(true, true)]
    public Color lethalColor = new Color(1f, 0.15f, 0.15f, 1f);

    [ColorUsage(true, true)]
    public Color safeColor = new Color(0.4f, 1f, 1f, 1f);

    [Range(0f, 1f)]
    public float ghostAlpha = 0.045f;

    [Header("Visual Transition")]
    public float transitionDuration = 0.25f;

    private Collider2D hazardCollider;
    private SpriteRenderer spriteRenderer;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        hazardCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Collider must be a trigger for the Hazard kill path to work
        hazardCollider.isTrigger = true;
    }

    private void Start()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += OnRealityChanged;
            // Initialise immediately with the current reality
            OnRealityChanged(RealityManager.Instance.currentReality);
        }
        else
        {
            ApplyLethalState(false);
        }
    }

    private void OnDestroy()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= OnRealityChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }

    private void OnRealityChanged(RealityManager.Reality newReality)
    {
        bool isLethal = IsLethalInReality(newReality);

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(TransitionRoutine(isLethal));
    }

    private bool IsLethalInReality(RealityManager.Reality reality)
    {
        return (lethalReality == LethalIn.RealityA && reality == RealityManager.Reality.A)
            || (lethalReality == LethalIn.RealityB && reality == RealityManager.Reality.B);
    }

    private IEnumerator TransitionRoutine(bool isLethal)
    {
        // Switch collider state immediately — never leave a kill trigger active mid-fade
        ApplyLethalState(isLethal);

        // Crossfade the sprite color
        Color startColor = spriteRenderer.color;
        Color endColor = BuildTargetColor(isLethal);
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(startColor, endColor, elapsed / transitionDuration);
            yield return null;
        }

        spriteRenderer.color = endColor;
        transitionRoutine = null;
    }

    private void ApplyLethalState(bool isLethal)
    {
        hazardCollider.enabled = isLethal;
    }

    private Color BuildTargetColor(bool isLethal)
    {
        if (isLethal)
        {
            Color c = lethalColor;
            c.a = 1f;
            return c;
        }
        else
        {
            Color c = safeColor * 0.5f;
            c.a = ghostAlpha;
            return c;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Preview the lethal color in the editor for easier prefab setup
            Color c = lethalColor;
            c.a = 1f;
            sr.color = c;
        }
    }
#endif
}
