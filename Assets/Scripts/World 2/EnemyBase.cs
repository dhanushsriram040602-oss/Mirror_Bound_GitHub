using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract base class for all World 2 enemies.
/// Owns reality subscription, ghost alpha toggling, and shared TryKill logic.
/// Subclasses implement OnRealityActivated / OnRealityDeactivated for custom behaviour.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public abstract class EnemyBase : MonoBehaviour
{
    // ── Reality Settings ─────────────────────────────────────────────────

    [Header("Reality Settings")]
    public DualRealityObject.ObjectType enemyReality = DualRealityObject.ObjectType.Both;

    [ColorUsage(true, true)]
    public Color colorA = new Color(1f, 0.5f, 0f) * 2f;

    [ColorUsage(true, true)]
    public Color colorB = new Color(0f, 0.9f, 1f) * 2f;

    [ColorUsage(true, true)]
    public Color colorBoth = Color.white * 2f;

    [Range(0f, 1f)]
    public float ghostAlpha = 0.045f;

    // ── Shared State (readable by subclasses) ────────────────────────────

    protected bool isActiveInCurrentReality = true;
    protected RealityManager.Reality currentReality = RealityManager.Reality.A;

    // ── Private Components ───────────────────────────────────────────────

    protected SpriteRenderer spriteRenderer;

    /// <summary>Trigger collider used for kill detection. Toggled off when ghosted.</summary>
    protected Collider2D mainCollider;

    /// <summary>
    /// Non-trigger collider used purely for physics grounding.
    /// Never toggled — enemy stays on the floor in both realities.
    /// </summary>
    protected Collider2D physicsCollider;

    private Color cachedBaseColor;

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    protected void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Sort colliders: trigger → mainCollider (kill, toggled by reality)
        //                 non-trigger → physicsCollider (grounding, never toggled)
        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            if (col.isTrigger)
                mainCollider = col;
            else
                physicsCollider = col;
        }

        CacheBaseColor();
        OnAwake();
    }

    protected void Start()
    {
        SubscribeToReality();
        OnStart();
    }

    protected void OnDestroy()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= HandleRealityChange;
        }
    }

    // ── Subclass Hooks ───────────────────────────────────────────────────

    /// <summary>Called when this enemy's reality matches the current reality.</summary>
    protected abstract void OnRealityActivated();

    /// <summary>Called when this enemy is ghosted (wrong reality).</summary>
    protected abstract void OnRealityDeactivated();

    /// <summary>Replaces Awake() in subclasses — called before reality subscription.</summary>
    protected virtual void OnAwake() { }

    /// <summary>Replaces Start() in subclasses — called after reality subscription.</summary>
    protected virtual void OnStart() { }

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>
    /// Forces a re-evaluation of the enemy's active state against the current reality.
    /// Used by PhaseWraith when it mutates its own enemyReality at runtime.
    /// </summary>
    public void ForceRealityEvaluation()
    {
        if (RealityManager.Instance != null)
        {
            HandleRealityChange(RealityManager.Instance.currentReality);
        }
    }

    // ── Kill Logic ───────────────────────────────────────────────────────

    /// <summary>Kills the player if the collider belongs to the Player tag and this enemy is active in the current reality.</summary>
    protected void TryKill(Collider2D col)
    {
        if (!isActiveInCurrentReality) return;
        if (!col.CompareTag("Player")) return;

        PlayerController player = col.GetComponent<PlayerController>();
        player?.Die();
    }

    // ── Reality Handling ─────────────────────────────────────────────────

    private void SubscribeToReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += HandleRealityChange;
            HandleRealityChange(RealityManager.Instance.currentReality);
        }
        else
        {
            // No manager yet — default to active/visible
            ApplyActiveVisual();
        }
    }

    protected void HandleRealityChange(RealityManager.Reality newReality)
    {
        currentReality           = newReality;
        isActiveInCurrentReality = DetermineActiveState(newReality);

        if (isActiveInCurrentReality)
        {
            // Restore physics before visuals so the enemy lands correctly.
            if (physicsCollider != null) physicsCollider.enabled = true;
            if (mainCollider    != null) mainCollider.enabled    = true;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

            ApplyActiveVisual();
            OnRealityActivated();
        }
        else
        {
            // Disable ALL colliders — enemy becomes fully passthrough.
            // Switch to Kinematic so gravity doesn't pull it off the floor.
            if (mainCollider    != null) mainCollider.enabled    = false;
            if (physicsCollider != null) physicsCollider.enabled = false;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType       = RigidbodyType2D.Kinematic;
            }

            ApplyGhostVisual();
            OnRealityDeactivated();
        }
    }

    private bool DetermineActiveState(RealityManager.Reality reality)
    {
        switch (enemyReality)
        {
            case DualRealityObject.ObjectType.Both:    return true;
            case DualRealityObject.ObjectType.RealityA: return reality == RealityManager.Reality.A;
            case DualRealityObject.ObjectType.RealityB: return reality == RealityManager.Reality.B;
            default: return true;
        }
    }

    // ── Visuals ──────────────────────────────────────────────────────────

    private void CacheBaseColor()
    {
        switch (enemyReality)
        {
            case DualRealityObject.ObjectType.RealityA: cachedBaseColor = colorA;    break;
            case DualRealityObject.ObjectType.RealityB: cachedBaseColor = colorB;    break;
            default:                                     cachedBaseColor = colorBoth; break;
        }
    }

    private void ApplyActiveVisual()
    {
        if (spriteRenderer == null) return;
        Color c = cachedBaseColor;
        c.a = 1f;
        spriteRenderer.color = c;
    }

    private void ApplyGhostVisual()
    {
        if (spriteRenderer == null) return;
        Color c = cachedBaseColor * 0.5f;
        c.a = ghostAlpha;
        spriteRenderer.color = c;
    }

    /// <summary>
    /// Recaches base color and refreshes the active visual. Call after
    /// mutating enemyReality at runtime (e.g. PhaseWraith phase swap).
    /// </summary>
    protected void RefreshBaseColor()
    {
        CacheBaseColor();
    }

#if UNITY_EDITOR
    protected void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        CacheBaseColor();
        ApplyActiveVisual();
    }
#endif
}
