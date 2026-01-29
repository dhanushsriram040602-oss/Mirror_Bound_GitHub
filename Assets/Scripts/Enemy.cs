using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    Vector3 startPos;
    int direction = 1;
    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        startPos = transform.position;
    }

    void Update()
    {
        float offset = transform.position.x - startPos.x;

        if (offset > patrolDistance && direction > 0)
            TurnAround();
        else if (offset < -patrolDistance && direction < 0)
            TurnAround();

        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    void TurnAround()
    {
        direction *= -1;
        if (sr != null)
            sr.flipX = direction < 0;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    void TryKill(Collider2D col)
    {
        if (!col.CompareTag("Player"))
            return;

        PlayerCtrl player = col.GetComponent<PlayerCtrl>();
        if (player != null)
        {
            player.Die();
        }
    }
}
