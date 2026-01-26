using UnityEngine;
using UnityEngine.SceneManagement;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float patrolDistance = 3f;

    private Vector3 startPos;
    private int direction = 1;
    private SpriteRenderer sr;
    private string currentSceneName;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        startPos = transform.position;
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        float distanceFromStart = transform.position.x - startPos.x;

        if (distanceFromStart > patrolDistance && direction > 0)
        {
            direction = -1;
            FlipSprite();
        }
        else if (distanceFromStart < -patrolDistance && direction < 0)
        {
            direction = 1;
            FlipSprite();
        }

        transform.Translate(Vector2.right * direction * speed * Time.deltaTime);
    }

    private void FlipSprite()
    {
        if (sr != null)
        {
            sr.flipX = (direction < 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.Die();
            }
        }
    }
}