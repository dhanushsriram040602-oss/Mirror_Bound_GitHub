using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StarAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("How long each star takes to animate in")]
    public float animationDuration = 0.5f;
    
    [Tooltip("Delay between each star animation")]
    public float delayBetweenStars = 0.15f;
    
    [Tooltip("Scale multiplier for pop effect")]
    public float popScale = 1.3f;
    
    [Tooltip("Animation curve for bounce effect")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Audio (Optional)")]
    public AudioClip starPopSound;
    public AudioSource audioSource;

    [Header("Colors")]
    public Color filledColor = Color.yellow;
    public Color emptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

    private Image[] starImages;
    private Dictionary<Image, Vector3> originalScales = new Dictionary<Image, Vector3>();

    void Awake()
    {
        starImages = GetComponentsInChildren<Image>(true);
        
        if (starImages.Length == 0)
        {
            Debug.LogWarning($"[StarAnimator] No star images found on {gameObject.name}");
        }
        
        foreach (Image starImage in starImages)
        {
            if (starImage != null && starImage.gameObject.name.Contains("Star"))
            {
                originalScales[starImage] = starImage.transform.localScale;
            }
        }
    }

    public void SetStars(int starCount, bool animated = true)
    {
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (!animated)
        {
            SetStarsImmediate(starCount);
            return;
        }

        foreach (Image starImage in starImages)
        {
            if (starImage != null && starImage.gameObject.name.Contains("Star"))
            {
                starImage.color = emptyColor;
            }
        }

        StopAllCoroutines();
        StartCoroutine(AnimateStars(starCount));
    }

    void SetStarsImmediate(int starCount)
    {
        for (int i = 0; i < starImages.Length && i < 3; i++)
        {
            if (starImages[i] != null && starImages[i].gameObject.name.Contains("Star"))
            {
                starImages[i].color = (i < starCount) ? filledColor : emptyColor;
                
                if (originalScales.ContainsKey(starImages[i]))
                {
                    starImages[i].transform.localScale = originalScales[starImages[i]];
                }
            }
        }
    }

    IEnumerator AnimateStars(int starCount)
    {
        int starIndex = 0;

        foreach (Image starImage in starImages)
        {
            if (starImage == null || !starImage.gameObject.name.Contains("Star"))
                continue;

            if (starIndex >= 3)
                break;

            bool shouldFill = starIndex < starCount;
            
            Vector3 targetScale = originalScales.ContainsKey(starImage) ? originalScales[starImage] : Vector3.one;
            
            starImage.color = emptyColor;
            starImage.transform.localScale = Vector3.zero;

            if (shouldFill)
            {
                yield return new WaitForSecondsRealtime(delayBetweenStars);
                StartCoroutine(AnimateSingleStar(starImage, filledColor, targetScale));
            }
            else
            {
                starImage.color = emptyColor;
                starImage.transform.localScale = targetScale;
            }

            starIndex++;
        }
    }

    IEnumerator AnimateSingleStar(Image starImage, Color targetColor, Vector3 finalScale)
    {
        PlayStarSound();

        float elapsed = 0f;
        Color startColor = emptyColor;
        float maxScale = finalScale.x * popScale;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;

            float curveValue = scaleCurve.Evaluate(t);
            float currentScale = Mathf.Lerp(0f, maxScale, curveValue);
            
            if (t > 0.6f)
            {
                float bounceT = (t - 0.6f) / 0.4f;
                currentScale = Mathf.Lerp(maxScale, finalScale.x, bounceT);
            }

            starImage.transform.localScale = Vector3.one * currentScale;
            starImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        starImage.transform.localScale = finalScale;
        starImage.color = targetColor;
    }

    void PlayStarSound()
    {
        if (starPopSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(starPopSound);
        }
    }

    public void SetColors(Color filled, Color empty)
    {
        filledColor = filled;
        emptyColor = empty;
    }
}
