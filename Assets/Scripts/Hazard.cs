using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryKill(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
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
