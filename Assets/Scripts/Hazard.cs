using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDie();
                }
                player.Die();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDie();
                }
                player.Die();
            }
        }
    }
}