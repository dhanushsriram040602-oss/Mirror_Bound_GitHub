using UnityEngine;
using TMPro; // Required for TextMeshPro

public class BlinkingEffect : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 2.0f;   // How fast it blinks
    public float minAlpha = 0.1f; // How transparent it gets (0 = invisible)
    public float maxAlpha = 1.0f; // How opaque it gets (1 = fully visible)

    private TextMeshProUGUI textComp;

    void Awake()
    {
        textComp = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (textComp != null)
        {
            // Calculate alpha value using a Sine wave for smooth pulsing
            // Mathf.PingPong can also work, but Sin is often smoother
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f);

            // Apply new color
            Color c = textComp.color;
            c.a = alpha;
            textComp.color = c;
        }
    }
}