using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Displays the pre-cutscene loading screen: black background, player sprite at centre,
/// a blinking eye overlay on the player, "Touch the eye to continue" hint text, and an
/// Into the Spider-Verse-style glitch effect (chromatic aberration, scanlines, halftone dots).
/// Touching the eye triggers a fade-out and signals the OpeningCutsceneController to begin.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class LoadingSceneController : MonoBehaviour
{
    // ─────────────────────────── Inspector ────────────────────────────
    [Header("References")]
    [Tooltip("The player sprite to display centre-screen.")]
    public Sprite playerSprite;

    [Tooltip("The OpeningCutsceneController to activate when the player taps the eye.")]
    public OpeningCutsceneController cutsceneController;

    [Header("Player Display")]
    public Vector2 playerSize = new Vector2(220f, 220f);
    public Vector2 playerAnchoredPosition = Vector2.zero;

    [Header("Eye (single black pupil)")]
    [Tooltip("Diameter of the black pupil circle in pixels.")]
    public float eyeDiameter = 38f;
    [Tooltip("Position of the pupil relative to the player image centre.")]
    public Vector2 eyeOffset = new Vector2(0f, 22f);
    [Tooltip("Multiplier for the invisible touch hitbox. 1.8 gives a generously tappable area.")]
    public float eyeHitboxScale = 1.8f;

    [Header("Blink Timing")]
    [Tooltip("Seconds the eye stays open between blinks.")]
    public float openDuration = 3f;
    [Tooltip("Time in seconds to close the eye.")]
    public float closeTime = 0.12f;
    [Tooltip("Time in seconds to reopen the eye.")]
    public float openTime = 0.18f;

    [Header("Hint Text")]
    public string hintText = "Touch the eye to continue";
    public float hintFontSize = 28f;
    public Color hintColor = new Color(0.85f, 0.85f, 0.85f, 1f);
    [Tooltip("Distance in pixels from the bottom edge of the screen.")]
    public float hintBottomPadding = 60f;

    [Header("Glitch (Spider-Verse)")]
    [Range(0f, 1f)] public float glitchIntensity = 0.55f;
    [Tooltip("Average seconds between glitch bursts.")]
    public float glitchInterval = 2.2f;
    [Tooltip("Duration of a single glitch burst.")]
    public float glitchDuration = 0.22f;

    [Header("Transition")]
    [Tooltip("Seconds the eye takes to zoom in and cover the screen.")]
    public float zoomIntoDuration = 0.9f;

    // ─────────────────────────── Runtime ──────────────────────────────
    private Canvas canvas;
    private CanvasGroup rootGroup;
    private Image playerImage;
    private Image eyeImage;
    private TextMeshProUGUI hintLabel;

    // Glitch layers – three coloured ghost copies of the player
    private Image glitchRed;
    private Image glitchCyan;
    private RawImage scanlineOverlay;
    private RawImage halftoneOverlay;

    // Blink coroutine
    private Coroutine blinkRoutine;
    private Coroutine glitchRoutine;
    private bool interactionEnabled = false;

    // Glitch state (used in Update for per-frame shake)
    private bool isGlitching = false;
    private float glitchTimer = 0f;

    // ─────────────────────────── Constants ────────────────────────────
    private static readonly Color RedAberration = new Color(1f, 0f, 0.26f, 0.45f);
    private static readonly Color CyanAberration = new Color(0f, 0.82f, 1f, 0.45f);

    // ═══════════════════════════ Lifecycle ════════════════════════════

    private void Awake()
    {
        // In the Editor always show the loading screen so it can be tested freely.
        // In a build it only shows on the very first launch (PlayerPrefs flag not yet set).
#if UNITY_EDITOR
        bool shouldShow = true;
#else
        bool shouldShow = PlayerPrefs.GetInt("HasSeenIntro", 0) == 0;
#endif

        if (!shouldShow)
        {
            gameObject.SetActive(false);
            return;
        }

        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 99;

        if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        rootGroup = gameObject.GetComponent<CanvasGroup>();
        if (rootGroup == null)
            rootGroup = gameObject.AddComponent<CanvasGroup>();

        rootGroup.alpha = 1f;
        rootGroup.blocksRaycasts = true;
    }

    /// <summary>Clears the intro flag so the full loading screen + cutscene plays again on next build launch.</summary>
    [ContextMenu("Reset Intro Flag")]
    private void ResetFirstLaunchFlag()
    {
        PlayerPrefs.DeleteKey("HasSeenIntro");
        PlayerPrefs.Save();
        Debug.Log("LoadingSceneController: HasSeenIntro flag cleared — intro will play on next build launch.");
    }

    private void Start()
    {
        // Awake already deactivated us on non-first launches, so if we reach Start
        // the loading screen is definitely needed.
        BuildUI();
        StartBlinkLoop();
        glitchRoutine = StartCoroutine(GlitchBurstLoop());
        interactionEnabled = true;
    }

    private void Update()
    {
        AnimateHintPulse();
        UpdateGlitchFrame();
    }

    // ═══════════════════════════ UI Construction ══════════════════════

    private void BuildUI()
    {
        // ── Camera background → black
        Camera cam = Camera.main;
        if (cam != null)
            cam.backgroundColor = Color.black;

        // ── Root panel (full-screen, transparent catch-all)
        GameObject panelGO = new GameObject("LoadingPanel");
        panelGO.transform.SetParent(transform, false);
        Image panel = panelGO.AddComponent<Image>();
        panel.color = Color.black;
        panel.raycastTarget = true;
        RectTransform panelRT = panel.rectTransform;
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;

        // ── Player image
        GameObject playerGO = new GameObject("Player");
        playerGO.transform.SetParent(transform, false);
        playerImage = playerGO.AddComponent<Image>();
        playerImage.sprite = playerSprite;
        playerImage.preserveAspect = true;
        playerImage.raycastTarget = false;
        SetAnchored(playerImage.rectTransform, playerSize, playerAnchoredPosition);

        // ── Glitch shadow images (below eye, above player)
        glitchRed = CreateGhostImage("GlitchRed", RedAberration);
        glitchCyan = CreateGhostImage("GlitchCyan", CyanAberration);
        glitchRed.enabled = false;
        glitchCyan.enabled = false;

        // ── Scanline overlay
        scanlineOverlay = CreateScanlineTexture();

        // ── Halftone dot overlay
        halftoneOverlay = CreateHalftoneTexture();

        // ── Eye image (interactive)
        GameObject eyeGO = new GameObject("Eye");
        eyeGO.transform.SetParent(transform, false);
        eyeImage = eyeGO.AddComponent<Image>();

        // Procedural filled-circle sprite for the pupil
        eyeImage.sprite = CreateCircleSprite(64, Color.black);
        eyeImage.color = Color.black;
        eyeImage.preserveAspect = false;
        eyeImage.raycastTarget = false;     // visual only – hitbox is separate
        Vector2 eyeAnchorPos = playerAnchoredPosition + eyeOffset;
        float d = eyeDiameter;
        SetAnchored(eyeImage.rectTransform, new Vector2(d, d), eyeAnchorPos);

        // Invisible touch hitbox (larger circle on top of the pupil)
        GameObject hitGO = new GameObject("EyeHitbox");
        hitGO.transform.SetParent(transform, false);
        Image hitImage = hitGO.AddComponent<Image>();
        hitImage.sprite = CreateCircleSprite(64, Color.white);
        hitImage.color = new Color(1f, 1f, 1f, 0f);   // fully transparent
        hitImage.raycastTarget = true;
        float hd = d * eyeHitboxScale;
        SetAnchored(hitImage.rectTransform, new Vector2(hd, hd), eyeAnchorPos);

        // Pointer click handler on the hitbox
        EventTrigger trigger = hitGO.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        entry.callback.AddListener(_ => OnEyeTouched());
        trigger.triggers.Add(entry);

        // ── Hint text — anchored to the bottom of the screen
        GameObject textGO = new GameObject("HintText");
        textGO.transform.SetParent(transform, false);
        hintLabel = textGO.AddComponent<TextMeshProUGUI>();
        hintLabel.text = hintText;
        hintLabel.fontSize = hintFontSize;
        hintLabel.color = hintColor;
        hintLabel.alignment = TextAlignmentOptions.Center;
        hintLabel.raycastTarget = false;
        RectTransform textRT = hintLabel.rectTransform;
        // Anchor to bottom-centre, sit hintBottomPadding pixels above the bottom edge
        textRT.anchorMin = new Vector2(0f, 0f);
        textRT.anchorMax = new Vector2(1f, 0f);
        textRT.pivot = new Vector2(0.5f, 0f);
        textRT.sizeDelta = new Vector2(0f, 60f);
        textRT.anchoredPosition = new Vector2(0f, hintBottomPadding);
    }

    // ─────────────────────────── Helpers ──────────────────────────────

    private Image CreateGhostImage(string goName, Color tint)
    {
        GameObject go = new GameObject(goName);
        go.transform.SetParent(transform, false);
        Image img = go.AddComponent<Image>();
        img.sprite = playerSprite;
        img.preserveAspect = true;
        img.color = tint;
        img.raycastTarget = false;
        SetAnchored(img.rectTransform, playerSize, playerAnchoredPosition);
        return img;
    }

    private RawImage CreateScanlineTexture()
    {
        // Thin horizontal 2-pixel stripe repeated every 4 pixels → scanline feel
        const int texHeight = 4;
        Texture2D tex = new Texture2D(1, texHeight, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        tex.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.25f));
        tex.SetPixel(0, 1, new Color(0f, 0f, 0f, 0.25f));
        tex.SetPixel(0, 2, Color.clear);
        tex.SetPixel(0, 3, Color.clear);
        tex.Apply();

        GameObject go = new GameObject("Scanlines");
        go.transform.SetParent(transform, false);
        RawImage img = go.AddComponent<RawImage>();
        img.texture = tex;
        img.uvRect = new Rect(0f, 0f, 1f, Screen.height / (float)texHeight);
        img.color = Color.white;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        img.enabled = false;        // only shown during glitch bursts
        return img;
    }

    private RawImage CreateHalftoneTexture()
    {
        // 4×4 pattern with a single bright dot → halftone comic feel
        const int s = 4;
        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                tex.SetPixel(x, y, Color.clear);
        tex.SetPixel(1, 1, new Color(1f, 1f, 1f, 0.08f));  // sparse dot
        tex.Apply();

        GameObject go = new GameObject("Halftone");
        go.transform.SetParent(transform, false);
        RawImage img = go.AddComponent<RawImage>();
        img.texture = tex;
        img.uvRect = new Rect(0f, 0f, Screen.width / (float)s, Screen.height / (float)s);
        img.color = Color.white;
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        img.enabled = true;         // always on, subtle
        return img;
    }

    private static void SetAnchored(RectTransform rt, Vector2 size, Vector2 anchoredPos)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
    }

    /// <summary>Generates a filled-circle sprite at runtime so no sprite asset is required.</summary>
    private static Sprite CreateCircleSprite(int resolution, Color fill)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float half = resolution * 0.5f;
        float r2 = half * half;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = x - half + 0.5f;
                float dy = y - half + 0.5f;
                float aa = Mathf.Clamp01((r2 - (dx * dx + dy * dy)) / (half * 1.5f) + 0.5f);
                tex.SetPixel(x, y, new Color(fill.r, fill.g, fill.b, aa));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
    }

    // ═══════════════════════════ Blink Loop ═══════════════════════════

    private void StartBlinkLoop()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(BlinkLoop());
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(openDuration);
            yield return StartCoroutine(LerpEyeAlpha(1f, 0f, closeTime));
            yield return new WaitForSeconds(0.04f);
            yield return StartCoroutine(LerpEyeAlpha(0f, 1f, openTime));
        }
    }

    private IEnumerator LerpEyeAlpha(float from, float to, float duration)
    {
        if (eyeImage == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Squash the pupil vertically — 0.05 is a thin line, 1 is fully round
            float scaleY = Mathf.Max(Mathf.Lerp(from, to, t), 0.05f);
            eyeImage.rectTransform.localScale = new Vector3(1f, scaleY, 1f);
            yield return null;
        }
        eyeImage.rectTransform.localScale = new Vector3(1f, Mathf.Max(to, 0.05f), 1f);
    }

    // ═══════════════════════════ Hint Pulse ═══════════════════════════

    private void AnimateHintPulse()
    {
        if (hintLabel == null) return;
        float alpha = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(Time.time * 1.8f) + 1f) * 0.5f);
        Color c = hintLabel.color;
        c.a = alpha;
        hintLabel.color = c;
    }

    // ═════════════════════ Glitch (Spider-Verse) ══════════════════════

    private IEnumerator GlitchBurstLoop()
    {
        while (true)
        {
            float wait = Random.Range(glitchInterval * 0.5f, glitchInterval * 1.5f);
            yield return new WaitForSeconds(wait);
            yield return StartCoroutine(RunGlitchBurst());
        }
    }

    private IEnumerator RunGlitchBurst()
    {
        isGlitching = true;
        float elapsed = 0f;

        if (scanlineOverlay != null) scanlineOverlay.enabled = true;

        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        isGlitching = false;
        if (scanlineOverlay != null) scanlineOverlay.enabled = false;
        if (glitchRed != null) { glitchRed.enabled = false; }
        if (glitchCyan != null) { glitchCyan.enabled = false; }

        // Reset player position after glitch
        if (playerImage != null)
            playerImage.rectTransform.anchoredPosition = playerAnchoredPosition;
    }

    /// <summary>Runs every frame; applies rapid shake/offset when glitch burst is active.</summary>
    private void UpdateGlitchFrame()
    {
        if (!isGlitching)
        {
            glitchTimer = 0f;
            return;
        }

        glitchTimer += Time.deltaTime;

        // Decide whether this frame has a glitch slice (not every frame — sparse, like Spider-Verse)
        bool showSlice = (Mathf.PerlinNoise(glitchTimer * 40f, 0f) > (1f - glitchIntensity));

        if (showSlice)
        {
            float offsetX = Random.Range(-18f, 18f) * glitchIntensity;
            float offsetY = Random.Range(-6f, 6f) * glitchIntensity;

            // Main player — jitter
            if (playerImage != null)
                playerImage.rectTransform.anchoredPosition = playerAnchoredPosition + new Vector2(offsetX * 0.3f, offsetY * 0.3f);

            // Red channel ghost — offset right
            if (glitchRed != null)
            {
                glitchRed.enabled = true;
                glitchRed.rectTransform.anchoredPosition = playerAnchoredPosition + new Vector2(offsetX, offsetY * 0.5f);
                Color rc = RedAberration;
                rc.a = Random.Range(0.3f, 0.6f);
                glitchRed.color = rc;
            }

            // Cyan channel ghost — offset left
            if (glitchCyan != null)
            {
                glitchCyan.enabled = true;
                glitchCyan.rectTransform.anchoredPosition = playerAnchoredPosition + new Vector2(-offsetX, -offsetY * 0.5f);
                Color cc = CyanAberration;
                cc.a = Random.Range(0.3f, 0.6f);
                glitchCyan.color = cc;
            }
        }
        else
        {
            // No slice this frame → restore
            if (playerImage != null)
                playerImage.rectTransform.anchoredPosition = playerAnchoredPosition;
            if (glitchRed != null) glitchRed.enabled = false;
            if (glitchCyan != null) glitchCyan.enabled = false;
        }
    }

    // ═══════════════════════════ Interaction ══════════════════════════

    private void OnEyeTouched()
    {
        if (!interactionEnabled) return;
        interactionEnabled = false;

        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        if (glitchRoutine != null) StopCoroutine(glitchRoutine);

        StartCoroutine(EyeTapSequence());
    }

    private IEnumerator EyeTapSequence()
    {
        // ── 1. Freeze blink — eye must stay open for the zoom
        eyeImage.rectTransform.localScale = Vector3.one;

        // ── 2. Brief intense glitch burst on tap
        isGlitching = true;
        if (scanlineOverlay != null) scanlineOverlay.enabled = true;
        yield return new WaitForSeconds(0.22f);
        isGlitching = false;
        if (scanlineOverlay != null) scanlineOverlay.enabled = false;
        if (glitchRed != null) glitchRed.enabled = false;
        if (glitchCyan != null) glitchCyan.enabled = false;

        // Snap player back to rest position after any glitch jitter
        if (playerImage != null)
            playerImage.rectTransform.anchoredPosition = playerAnchoredPosition;

        // Hide secondary UI so only the player + eye remain during zoom
        if (hintLabel != null) hintLabel.gameObject.SetActive(false);
        if (halftoneOverlay != null) halftoneOverlay.enabled = false;

        // Move eye on top of all siblings so nothing renders over it during zoom
        eyeImage.transform.SetAsLastSibling();

        // ── 3. Zoom — scale the eye outward until it covers the full screen
        //    The eye is a black circle; as it grows it naturally blacks out everything.
        float screenDiag = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height);
        float targetEyeScale = (screenDiag / eyeDiameter) * 1.5f;   // 1.5× safety margin for off-centre positions

        float elapsed = 0f;
        Vector3 playerStartScale = playerImage != null ? playerImage.rectTransform.localScale : Vector3.one;

        while (elapsed < zoomIntoDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomIntoDuration);

            // Cubic ease-in: slow start, accelerates hard → cinematic punch
            float eased = t * t * t;

            // Eye grows from pupil size to cover the screen
            float eyeScale = Mathf.Lerp(1f, targetEyeScale, eased);
            eyeImage.rectTransform.localScale = new Vector3(eyeScale, eyeScale, 1f);

            // Player uses the exact same scale — they zoom together as one unit
            if (playerImage != null)
                playerImage.rectTransform.localScale = new Vector3(eyeScale, eyeScale, 1f);

            yield return null;
        }

        // ── 4. Screen is fully black (eye covers everything).
        //    Signal the cutscene. Its transitionGroup starts at alpha=1 (black) and fades in,
        //    so the handoff is seamless with no visible gap.
        if (cutsceneController != null)
            cutsceneController.BeginCutsceneFromLoading();

        // Wait one frame so the cutscene's black overlay is in place before we disappear
        yield return null;

        gameObject.SetActive(false);
    }
}
