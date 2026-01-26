using UnityEngine;

public class GoldPlatform : MonoBehaviour
{
    [Header("Visual Settings")]
    public Color activeColor = new Color(1f, 0.8f, 0f, 1f);
    public Color inactiveColor = new Color(1f, 0.8f, 0f, 0.3f);

    [Header("Glow Effects")]
    public GameObject glowEffectObject;

    public bool usePulseAnimation = true;
    public float pulseSpeed = 3.0f;
    public float pulseBrightness = 0.4f;

    [Header("State")]
    [SerializeField] private bool isActivated = false;

    private SpriteRenderer spriteRend;
    private BoxCollider2D boxCol;

    void Awake()
    {
        CacheComponents();
        UpdateState();
    }

    void OnEnable()
    {
        SubscribeToReality();
        UpdateState();
    }

    void OnDisable()
    {
        UnsubscribeFromReality();
    }

    void Update()
    {
        if (isActivated && IsRealityA() && usePulseAnimation && spriteRend != null)
        {
            ApplyPulseEffect();
        }
    }

    public void Activate()
    {
        isActivated = true;
        UpdateState();
    }

    public void ResetPlatform()
    {
        isActivated = false;
        UpdateState();
    }

    public bool IsActivated()
    {
        return isActivated;
    }

    private void CacheComponents()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider2D>();
    }

    private void SubscribeToReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += OnRealityChanged;
        }
    }

    private void UnsubscribeFromReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= OnRealityChanged;
        }
    }

    private void OnRealityChanged(RealityManager.Reality newReality)
    {
        UpdateState();
    }

    private void UpdateState()
    {
        if (spriteRend == null || boxCol == null) return;

        bool shouldBeVisible = isActivated && IsRealityA();

        spriteRend.enabled = shouldBeVisible;
        boxCol.enabled = shouldBeVisible;

        if (shouldBeVisible)
        {
            spriteRend.color = activeColor;
            if (glowEffectObject != null)
            {
                glowEffectObject.SetActive(true);
            }
        }
        else
        {
            spriteRend.color = inactiveColor;
            if (glowEffectObject != null)
            {
                glowEffectObject.SetActive(false);
            }
        }
    }

    private void ApplyPulseEffect()
    {
        float brightness = 0.8f + Mathf.PingPong(Time.time * pulseSpeed, pulseBrightness);

        Color finalColor = activeColor;
        finalColor.r *= brightness;
        finalColor.g *= brightness;
        finalColor.b *= brightness;

        spriteRend.color = finalColor;
    }

    private bool IsRealityA()
    {
        return RealityManager.Instance != null &&
               RealityManager.Instance.currentReality == RealityManager.Reality.A;
    }
}
