using UnityEngine;

[RequireComponent(typeof(GoldPlatform))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class GoldPlatformRealityVisibility : MonoBehaviour
{
    [Header("Reality Rule")]
    public RealityManager.Reality visibleInReality = RealityManager.Reality.A;

    private GoldPlatform goldPlatform;
    private SpriteRenderer rend;
    private BoxCollider2D col;

    void Awake()
    {
        goldPlatform = GetComponent<GoldPlatform>();
        rend = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
    }

    void OnEnable()
    {
        if (RealityManager.Instance != null)
            RealityManager.Instance.OnRealityChange += OnRealityChanged;
    }

    void OnDisable()
    {
        if (RealityManager.Instance != null)
            RealityManager.Instance.OnRealityChange -= OnRealityChanged;
    }

    void Start()
    {
        UpdateVisibility();
    }

    private void OnRealityChanged(RealityManager.Reality newReality)
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (RealityManager.Instance == null) return;

        bool shouldBeVisible =
            goldPlatform.IsActivated() &&
            RealityManager.Instance.currentReality == visibleInReality;

        rend.enabled = shouldBeVisible;
        col.enabled  = shouldBeVisible;
    }
}
