using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Cinematic "Reality Fracture" opening transition.
///
/// Sequence:
///   1. Divider charges up  — pulses wide, snaps back (energy build-up)
///   2. Impact crack        — divider surges to full screen width + instant screen flash
///   3. Panels launch       — Orange fires left, Blue fires right (EaseInExpo)
///                            both panels fade out as they exit
///   4. Divider collapses   — scale X shrinks to zero while panels split
///   5. Reveal flash fades  — callback fires
/// </summary>
public class SplitScreenTransition : MonoBehaviour
{
    [Header("Panels")]
    public RectTransform realityAPanel;
    public RectTransform realityBPanel;
    public RectTransform divider;

    [Header("Flash Overlay")]
    [Tooltip("A full-screen white Image placed as the last child of SplitScreenBackground.")]
    public Image flashOverlay;

    [Header("Timing")]
    [Tooltip("Duration of the divider charge-up pulse.")]
    public float chargeDuration = 0.28f;
    [Tooltip("Duration of the divider impact surge.")]
    public float surgeDuration = 0.08f;
    [Tooltip("Duration panels take to exit screen.")]
    public float slideDuration = 0.62f;
    [Tooltip("How long the reveal flash takes to fade out.")]
    public float flashFadeOut = 0.35f;
    [Tooltip("Stagger delay between Orange launch and Blue launch.")]
    public float panelStagger = 0.04f;

    [Header("Divider Charge Scale")]
    [Tooltip("How many times wider the divider pulses during charge-up.")]
    public float chargeMaxScaleX = 5f;
    [Tooltip("Scale X the divider settles at after the charge, before the surge.")]
    public float chargeRestScaleX = 2f;

    [Header("Callback")]
    public UnityEvent onTransitionComplete;

    // ?? Private state ?????????????????????????????????????????????????????????
    private RectTransform _canvasRect;
    private bool _isPlaying;

    // Cached initial state so panels can be reset on replay
    private Vector2 _aOriginPos;
    private Vector2 _bOriginPos;
    private Vector3 _dividerOriginScale;
    private Color _aOriginColor;
    private Color _bOriginColor;

    // ?? Unity lifecycle ???????????????????????????????????????????????????????
    void Awake()
    {
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            foreach (Canvas c in GetComponentsInParent<Canvas>())
            {
                if (c.isRootCanvas) { rootCanvas = c; break; }
            }
            _canvasRect = rootCanvas.GetComponent<RectTransform>();
        }

        // Cache original positions and colors for replay
        if (realityAPanel != null)
        {
            _aOriginPos = realityAPanel.anchoredPosition;
            Image img = realityAPanel.GetComponent<Image>();
            _aOriginColor = img != null ? img.color : Color.white;
        }

        if (realityBPanel != null)
        {
            _bOriginPos = realityBPanel.anchoredPosition;
            Image img = realityBPanel.GetComponent<Image>();
            _bOriginColor = img != null ? img.color : Color.white;
        }

        if (divider != null)
            _dividerOriginScale = divider.localScale;

        if (flashOverlay != null)
        {
            SetAlpha(flashOverlay, 0f);
            flashOverlay.gameObject.SetActive(false);
        }
    }

    // ?? Public API ????????????????????????????????????????????????????????????

    /// <summary>Wire this to the Start Journey button's onClick event.</summary>
    public void PlayTransition()
    {
        // Always re-activate and reset so the animation replays correctly
        // after the user navigates back to the main menu
        gameObject.SetActive(true);
        ResetToInitialState();

        if (_isPlaying) return;
        StartCoroutine(RealityFractureSequence());
    }

    /// <summary>
    /// Re-enables the background and resets all panels to their original state.
    /// Call this whenever the main menu becomes visible again (e.g. back button).
    /// </summary>
    public void ShowBackground()
    {
        gameObject.SetActive(true);
        ResetToInitialState();
    }

    /// <summary>Resets all panels to their original state. Called before each replay.</summary>
    private void ResetToInitialState()
    {
        _isPlaying = false;

        if (realityAPanel != null)
        {
            realityAPanel.anchoredPosition = _aOriginPos;
            Image img = realityAPanel.GetComponent<Image>();
            if (img != null) img.color = _aOriginColor;
        }

        if (realityBPanel != null)
        {
            realityBPanel.anchoredPosition = _bOriginPos;
            Image img = realityBPanel.GetComponent<Image>();
            if (img != null) img.color = _bOriginColor;
        }

        if (divider != null)
            divider.localScale = _dividerOriginScale;

        if (flashOverlay != null)
        {
            SetAlpha(flashOverlay, 0f);
            flashOverlay.gameObject.SetActive(false);
        }
    }

    // ?? Main sequence ?????????????????????????????????????????????????????????

    private IEnumerator RealityFractureSequence()
    {
        _isPlaying = true;

        float canvasWidth = _canvasRect != null ? _canvasRect.rect.width : 800f;

        // Phase 1: Divider charges up
        if (divider != null)
            yield return StartCoroutine(DividerCharge());

        // Phase 2: Impact surge + instant screen flash
        yield return StartCoroutine(ImpactSurge(canvasWidth));

        // Phase 3 & 4: Panels launch + divider collapses (all parallel)
        Vector2 aStart = realityAPanel != null ? realityAPanel.anchoredPosition : Vector2.zero;
        Vector2 bStart = realityBPanel != null ? realityBPanel.anchoredPosition : Vector2.zero;
        Vector2 aEnd = aStart + new Vector2(-canvasWidth * 1.1f, 0f);
        Vector2 bEnd = bStart + new Vector2(canvasWidth * 1.1f, 0f);

        Coroutine slideA = StartCoroutine(SlidePanel(realityAPanel, aStart, aEnd, 0f));
        Coroutine slideB = StartCoroutine(SlidePanel(realityBPanel, bStart, bEnd, panelStagger));
        Coroutine collapse = StartCoroutine(DividerCollapse());

        yield return slideA;
        yield return slideB;
        yield return collapse;

        // Phase 5: Reveal flash fades away
        if (flashOverlay != null)
            yield return StartCoroutine(FadeImage(flashOverlay, flashOverlay.color.a, 0f, flashFadeOut));

        gameObject.SetActive(false);
        _isPlaying = false;
        onTransitionComplete?.Invoke();
    }

    // ?? Phase coroutines ??????????????????????????????????????????????????????

    private IEnumerator DividerCharge()
    {
        float halfDuration = chargeDuration * 0.5f;
        Vector3 origin = divider.localScale;
        Vector3 peak = new Vector3(chargeMaxScaleX, origin.y, origin.z);
        Vector3 settle = new Vector3(chargeRestScaleX, origin.y, origin.z);

        // Expand to peak
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / halfDuration));
            divider.localScale = Vector3.LerpUnclamped(origin, peak, t);
            yield return null;
        }

        // Contract to settle
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseOutQuad(Mathf.Clamp01(elapsed / halfDuration));
            divider.localScale = Vector3.LerpUnclamped(peak, settle, t);
            yield return null;
        }

        divider.localScale = settle;
    }

    private IEnumerator ImpactSurge(float canvasWidth)
    {
        if (divider != null)
        {
            float dividerWidth = divider.rect.width > 0f ? divider.rect.width : 8f;
            float surgeTargetX = canvasWidth / dividerWidth;
            Vector3 start = divider.localScale;
            Vector3 end = new Vector3(surgeTargetX, start.y, start.z);

            float elapsed = 0f;
            while (elapsed < surgeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOutExpo(Mathf.Clamp01(elapsed / surgeDuration));
                divider.localScale = Vector3.LerpUnclamped(start, end, t);
                yield return null;
            }
            divider.localScale = end;
        }

        // Instant flash on impact
        if (flashOverlay != null)
        {
            flashOverlay.gameObject.SetActive(true);
            SetAlpha(flashOverlay, 0.92f);
        }

        // Two frames so the flash registers visually
        yield return null;
        yield return null;
    }

    private IEnumerator SlidePanel(RectTransform panel, Vector2 from, Vector2 to, float delay)
    {
        if (panel == null) yield break;

        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Image img = panel.GetComponent<Image>();
        Color startColor = img != null ? img.color : Color.white;

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseInExpo(Mathf.Clamp01(elapsed / slideDuration));

            panel.anchoredPosition = Vector2.LerpUnclamped(from, to, t);

            // Fade out as panel exits — gives a ghosting trail feel
            if (img != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, EaseOutQuad(t));
                img.color = c;
            }

            yield return null;
        }

        panel.anchoredPosition = to;
        if (img != null) SetAlpha(img, 0f);
    }

    private IEnumerator DividerCollapse()
    {
        if (divider == null) yield break;

        Vector3 start = divider.localScale;
        Vector3 end = new Vector3(0f, start.y, start.z);
        float duration = slideDuration * 0.55f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = EaseInQuart(Mathf.Clamp01(elapsed / duration));
            divider.localScale = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        divider.localScale = end;
    }

    private IEnumerator FadeImage(Image image, float from, float to, float duration)
    {
        if (image == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(image, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetAlpha(image, to);
    }

    // ?? Easing functions ??????????????????????????????????????????????????????

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInExpo(float t) => t <= 0f ? 0f : Mathf.Pow(2f, 10f * t - 10f);
    private static float EaseOutExpo(float t) => t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    private static float EaseInQuart(float t) => t * t * t * t;

    // ?? Utility ???????????????????????????????????????????????????????????????

    private static void SetAlpha(Graphic g, float alpha)
    {
        Color c = g.color;
        c.a = alpha;
        g.color = c;
    }
}
