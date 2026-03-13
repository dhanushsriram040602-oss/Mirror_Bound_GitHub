using UnityEngine;
using System.Collections;

public class DelayedPlatformSwitch : MonoBehaviour
{
    [Header("Delay Settings")]
    public float activationDelay = 0.6f;

    [Header("Reality")]
    public bool activeInRealityA = true;

    private bool activated = false;
    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        // Start disabled
        sr.enabled = false;
        col.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        activated = true;
        StartCoroutine(ActivateAfterDelay());
    }

    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        sr.enabled = true;
        col.enabled = true;
    }
}
