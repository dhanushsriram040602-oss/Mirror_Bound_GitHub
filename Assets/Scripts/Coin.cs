using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Animation Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.5f;

    [Header("Effects")]
    public string collectParticlePool = "CoinCollect";

    private Vector3 startPos;
    private float bobOffset;

    void Start()
    {
        startPos = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        PlayCollectSound();
        SpawnCollectParticle();
        AddCoinToGame();
        
        Destroy(gameObject);
    }

    private void PlayCollectSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCoin();
        }
    }

    private void SpawnCollectParticle()
    {
        if (ParticlePoolManager.Instance != null)
        {
            ParticlePoolManager.Instance.SpawnParticle(collectParticlePool, transform.position);
        }
    }

    private void AddCoinToGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin();
        }
    }
}
