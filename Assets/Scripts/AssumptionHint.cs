using UnityEngine;
using TMPro;
using System.Collections;

public class AssumptionHint : MonoBehaviour
{
    public static AssumptionHint Instance;

    [Header("Hint Settings")]
    public int deathsBeforeHint = 3;
    public string hintMessage = "What if the world was already complete?";
    public float hintDisplayTime = 5f;

    [Header("UI References")]
    public TextMeshProUGUI hintText;
    public CanvasGroup hintCanvasGroup;

    // Static so it survives the scene reload that happens on every death.
    // Resets only when the level is actually changed (cleared by PlayerController.Awake).
    private static int deathCount = 0;
    private bool hintShown = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (hintCanvasGroup != null)
            hintCanvasGroup.alpha = 0f;
    }

    public void RegisterDeath()
    {
        if (hintShown) return;

        deathCount++;

        if (deathCount >= deathsBeforeHint)
            ShowHint();
    }

    /// <summary>
    /// Resets the death counter. Call this when the player enters a new level
    /// (not when reloading the same level after dying).
    /// </summary>
    public static void ResetDeathCount()
    {
        deathCount = 0;
    }

    void ShowHint()
    {
        if (hintShown) return;
        hintShown = true;

        if (hintText != null)
        {
            hintText.text = hintMessage;
        }

        StartCoroutine(FadeInHint());
    }

    IEnumerator FadeInHint()
    {
        float elapsed = 0f;
        float fadeDuration = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (hintCanvasGroup != null)
            {
                hintCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            }
            yield return null;
        }

        yield return new WaitForSeconds(hintDisplayTime);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            if (hintCanvasGroup != null)
            {
                hintCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            }
            yield return null;
        }
    }
}
