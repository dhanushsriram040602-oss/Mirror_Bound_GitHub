using UnityEngine;

public class SwitchController : MonoBehaviour
{
    [Header("Connections")]
    public TimedBridge targetBridge;

    [Header("Visuals")]
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.white;
    public ParticleSystem pressParticles;

    private bool isPressed = false;
    private SpriteRenderer spriteRend;

    void Awake()
    {
        spriteRend = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (spriteRend != null)
        {
            spriteRend.color = inactiveColor;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isPressed || !other.CompareTag("Player")) return;

        ActivateSwitch();
    }

    private void ActivateSwitch()
    {
        isPressed = true;

        UpdateVisuals();

        if (targetBridge != null)
        {
            targetBridge.TriggerBridge();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwitch();
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRend != null)
        {
            spriteRend.color = activeColor;
        }

        if (pressParticles != null)
        {
            pressParticles.Play();
        }
    }

    public void ResetSwitch()
    {
        isPressed = false;
        if (spriteRend != null)
        {
            spriteRend.color = inactiveColor;
        }
    }
}