using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reality-B-only mirror twin of the player.
/// Records the player's position every recordInterval seconds and applies the oldest
/// buffered position to itself, creating a 1.5-second delayed shadow.
/// Kills the player if they shift into Reality B while occupying the same space.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EchoShade : EnemyBase
{
    // ── Echo Settings ────────────────────────────────────────────────────

    [Header("Echo Settings")]
    public float recordInterval = 0.1f;
    public int   delayBufferSize = 15;  // 15 * 0.1s = 1.5 second delay

    [Header("References")]
    public PlayerController player;

    // ── Private State ────────────────────────────────────────────────────

    private readonly Queue<Vector3> positionBuffer = new Queue<Vector3>();
    private Coroutine recordRoutine;

    // ── EnemyBase Hooks ──────────────────────────────────────────────────

    protected override void OnAwake()
    {
        enemyReality = DualRealityObject.ObjectType.RealityB;
        colorB       = new Color(0f, 0.9f, 1f) * 2f;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        // Trigger collider for OnTriggerStay2D
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    protected override void OnStart()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }

        recordRoutine = StartCoroutine(RecordRoutine());
    }

    protected override void OnRealityActivated()
    {
        // Entering Reality B — shade becomes dangerous
    }

    protected override void OnRealityDeactivated()
    {
        // Leaving Reality B — shade is harmless ghost
    }

    // ── Unity Lifecycle ──────────────────────────────────────────────────

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isActiveInCurrentReality) return;
        TryKill(other);
    }

    // ── Record Coroutine ─────────────────────────────────────────────────

    private IEnumerator RecordRoutine()
    {
        var wait = new WaitForSeconds(recordInterval);

        while (true)
        {
            yield return wait;

            if (player == null) continue;

            positionBuffer.Enqueue(player.transform.position);

            if (positionBuffer.Count > delayBufferSize)
            {
                Vector3 replayPos = positionBuffer.Dequeue();
                transform.position = replayPos;
            }
        }
    }
}
