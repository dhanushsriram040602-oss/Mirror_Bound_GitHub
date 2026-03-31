using UnityEngine;

/// <summary>
/// Self-contained falling projectile spawned by CeilingCrawler.
/// Falls under gravity, kills the player on contact, and self-destructs after lifetime.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class RiftDropProjectile : MonoBehaviour
{
    [ColorUsage(true, true)]
    public Color projectileColor = Color.white * 2f;

    public float lifetime = 3f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType     = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            rb.freezeRotation = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = projectileColor;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        player?.Die();
    }
}
