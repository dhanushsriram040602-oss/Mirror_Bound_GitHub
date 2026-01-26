using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class SwitchTrigger : MonoBehaviour
{
    [Header("Connections")]
    public GoldPlatform targetPlatform;

    [Header("Timer")]
    public SwitchTimer switchTimer;


    [Header("Settings")]
    public float activationDelay = 0.6f;
    public Color pressedColor = Color.green;

    private bool isPressed = false;

    private SpriteRenderer rend;
    private BoxCollider2D col;
    private Color defaultColor;

    // 🔥 CRITICAL FLAG
    private bool initialized = false;

    void Awake()
    {
        rend = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();

        col.isTrigger = true;
        defaultColor = rend.color;

        // FORCE ENABLE AT AWAKE
        rend.enabled = true;
        col.enabled = true;
    }

    IEnumerator Start()
    {
        // Wait until RealityManager DEFINITELY exists
        while (RealityManager.Instance == null)
            yield return null;

        initialized = true;
        ApplyRealityState();
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

    void LateUpdate()
    {
        // 🔥 SAFETY NET: if something disables it, turn it back on
        if (!initialized) return;

        bool shouldBeVisible =
            RealityManager.Instance.currentReality == RealityManager.Reality.B;

        if (rend.enabled != shouldBeVisible)
            rend.enabled = shouldBeVisible;

        if (col.enabled != shouldBeVisible)
            col.enabled = shouldBeVisible;
    }

    void OnRealityChanged(RealityManager.Reality newReality)
    {
        ApplyRealityState();
    }

    private void ApplyRealityState()
    {
        if (RealityManager.Instance == null) return;

        bool visible =
            RealityManager.Instance.currentReality == RealityManager.Reality.B;

        rend.enabled = visible;
        col.enabled = visible;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPressed) return;
        if (!other.CompareTag("Player")) return;

        StartCoroutine(PressSequence());
    }

    IEnumerator PressSequence()
    {
        isPressed = true;
        rend.color = pressedColor;

        if (switchTimer != null)
            switchTimer.StartTimer();




        yield return new WaitForSeconds(activationDelay);

        if (targetPlatform != null)
            targetPlatform.Activate();

       
    }

    public void ResetSwitch()
    {
        isPressed = false;
        rend.color = defaultColor;
        ApplyRealityState();
    }
}
