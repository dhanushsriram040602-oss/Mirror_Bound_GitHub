using UnityEngine;

public class DualRealityObject : MonoBehaviour
{
    public enum ObjectType { RealityA, RealityB, Both }

    [Header("Configuration")]
    public ObjectType objectReality = ObjectType.RealityA;

    [Header("Visuals")]
    [ColorUsage(true, true)]
    public Color colorA = new Color(1f, 0.5f, 0f) * 2f;

    [ColorUsage(true, true)]
    public Color colorB = new Color(0f, 0.9f, 1f) * 2f;

    [ColorUsage(true, true)]
    public Color colorBoth = Color.white * 2f;

    [Header("Ghost Settings")]
    [Range(0f, 1f)]
    public float ghostAlpha = 0.045f;

    private SpriteRenderer sr;
    private Collider2D col;
    private Color baseColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        CacheBaseColor();
    }

    void Start()
    {
        SubscribeToReality();
    }

    void OnDestroy()
    {
        UnsubscribeFromReality();
    }

    private void CacheBaseColor()
    {
        switch (objectReality)
        {
            case ObjectType.RealityA:
                baseColor = colorA;
                break;
            case ObjectType.RealityB:
                baseColor = colorB;
                break;
            case ObjectType.Both:
                baseColor = colorBoth;
                break;
        }
    }

    private void SubscribeToReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += HandleRealityChange;
            HandleRealityChange(RealityManager.Instance.currentReality);
        }
        else
        {
            UpdateColor(true);
        }
    }

    private void UnsubscribeFromReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= HandleRealityChange;
        }
    }

    private void HandleRealityChange(RealityManager.Reality newReality)
    {
        bool isActive = DetermineActiveState(newReality);

        if (col != null)
        {
            col.enabled = isActive;
        }

        UpdateColor(isActive);
    }

    private bool DetermineActiveState(RealityManager.Reality currentReality)
    {
        if (objectReality == ObjectType.Both) return true;
        if (objectReality == ObjectType.RealityA && currentReality == RealityManager.Reality.A) return true;
        if (objectReality == ObjectType.RealityB && currentReality == RealityManager.Reality.B) return true;
        return false;
    }

    private void UpdateColor(bool isActive)
    {
        if (sr == null) return;

        if (isActive)
        {
            Color targetColor = baseColor;
            targetColor.a = 1f;
            sr.color = targetColor;
        }
        else
        {
            Color ghostColor = baseColor * 0.5f;
            ghostColor.a = ghostAlpha;
            sr.color = ghostColor;
        }
    }

    void OnValidate()
    {
        sr = GetComponent<SpriteRenderer>();
        CacheBaseColor();
        UpdateColor(true);
    }
}