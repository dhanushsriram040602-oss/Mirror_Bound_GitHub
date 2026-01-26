using UnityEngine;

public class TimedBridge : MonoBehaviour
{
    [Header("Timings")]
    [Tooltip("How long it takes to materialize (The 'Syncing...' phase). Set to 0 for instant solid.")]
    public float syncDuration = 0.2f;
    
    [Tooltip("How long the bridge stays solid")]
    public float stableDuration = 5.0f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    [Tooltip("Color when disabled (Alpha 0 makes it invisible)")]
    public Color offlineColor = new Color(1, 1, 1, 0f);
    public Color syncingColor = new Color(1, 0.8f, 0, 0.5f);
    public Color stableColor = new Color(1, 0.8f, 0, 1f);
    public Color criticalColor = new Color(1, 0, 0, 1f);

    [Header("Blink Settings")]
    public float criticalBlinkThreshold = 1.0f;
    public float blinkSpeed = 10f;

    private float triggerTime = -1f;
    private bool isActiveSequence = false;

    void Awake()
    {
        CacheComponents();
        UpdateState();
    }

    void OnEnable()
    {
        UpdateState();
    }

    void Update()
    {
        if (isActiveSequence)
        {
            UpdateState();
        }
    }

    public void TriggerBridge()
    {
        triggerTime = Time.time;
        isActiveSequence = true;

        if (gameObject.activeInHierarchy)
        {
            UpdateState();
        }
    }

    public void ResetBridge()
    {
        isActiveSequence = false;
        if (gameObject.activeInHierarchy)
        {
            UpdateState();
        }
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (boxCollider == null) boxCollider = GetComponent<BoxCollider2D>();
    }

    private void UpdateState()
    {
        if (spriteRenderer == null || boxCollider == null) return;

        if (!isActiveSequence)
        {
            SetVisuals(offlineColor, false);
            return;
        }

        float timeSinceTrigger = Time.time - triggerTime;

        if (timeSinceTrigger < syncDuration)
        {
            SetVisuals(syncingColor, false);
        }
        else if (timeSinceTrigger < syncDuration + stableDuration)
        {
            float timeLeft = (syncDuration + stableDuration) - timeSinceTrigger;

            if (timeLeft < criticalBlinkThreshold)
            {
                bool blink = Mathf.PingPong(Time.time * blinkSpeed, 1) > 0.5f;
                SetVisuals(blink ? criticalColor : stableColor, true);
            }
            else
            {
                SetVisuals(stableColor, true);
            }
        }
        else
        {
            isActiveSequence = false;
            SetVisuals(offlineColor, false);
        }
    }

    private void SetVisuals(Color color, bool solid)
    {
        spriteRenderer.color = color;
        
        if (boxCollider.enabled != solid)
        {
            boxCollider.enabled = solid;
        }
    }
}