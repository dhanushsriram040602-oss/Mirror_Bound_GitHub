using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpeningCutsceneController : MonoBehaviour
{
    [Header("Core")]
    public RectTransform uiRoot;
    public Camera worldCamera;

    [Header("Split Reality Backgrounds")]
    public PolygonGraphic realityA;
    public PolygonGraphic realityB;
    public Texture2D realityATexture;
    public Texture2D realityBTexture;

    [Header("Overlays")]
    public RawImage flashOverlay;
    public RawImage vignetteOverlay;
    public RawImage riftLine;

    [Header("Player Visuals")]
    public Image cube;
    public Image cubeShadowRed;
    public Image cubeShadowCyan;
    public Sprite playerSprite;
    public Sprite shadowSprite;

    [Header("Dialogue")]
    public TMP_Text dialogueText;
    public CanvasGroup dialogueGroup;

    [Header("Scene Transition")]
    public string mainMenuSceneName = "MainMenu";
    public bool loadMainMenuSceneAtEnd = true;
    public CanvasGroup transitionGroup;
    public Image transitionImage;
    public float introFadeFromBlackTime = 1.15f;
    public float exitFadeToBlackTime = 1.2f;
    public float finalHoldBeforeLoad = 0.1f;

    [Header("Optional")]
    public GameObject gameplayRoot;

    [Header("FX Counts")]
    [Range(0, 300)] public int speedLineCount = 120;
    [Range(0, 50)] public int glitchWaveCount = 12;

    private class SpeedLine
    {
        public LineRenderer lr;
        public float x;
        public float y;
        public float length;
        public float speed;
        public float thickness;
        public Color color;
        public float seed;
    }

    private class GlitchWave
    {
        public LineRenderer lr;
        public float baseY;
        public float thickness;
        public float seed;
        public Color color;
    }

    private readonly List<SpeedLine> speedLines = new List<SpeedLine>();
    private readonly List<GlitchWave> glitchWaves = new List<GlitchWave>();

    private Texture2D whiteTex;
    private Texture2D vignetteTex;
    private Sprite whiteSprite;
    private Material lineMaterial;

    private Vector2 uiBasePos;
    private Vector2 cubeBasePos;
    private Vector2 redBasePos;
    private Vector2 cyanBasePos;
    private Vector2 riftBasePos;

    private float cameraBaseOrthoSize;
    private float cameraBaseFov;

    private int phase = 0;
    private Coroutine dialogueRoutine;
    private bool initialized = false;

    private const float PHASE1_DIALOGUE_TIME = 3f;
    private const float PHASE2_DIALOGUE_TIME = 2.8f;
    private const float PHASE3_DIALOGUE_TIME = 1f;
    private const float PHASE4_DIALOGUE_TIME = 3f;
    private const float FADE_IN_TIME = 0.18f;
    private const float FADE_OUT_TIME = 0.35f;

    private static readonly Color RedShadowColor = new Color(1f, 0f, 0.26f, 1f);
    private static readonly Color RedShadowDim = new Color(1f, 0f, 0.26f, 0f);
    private static readonly Color CyanShadowColor = new Color(0f, 0.82f, 1f, 1f);
    private static readonly Color CyanShadowDim = new Color(0f, 0.82f, 1f, 0f);

    private void Awake()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
            return;
        }

        CreateRuntimeTextures();
        CreateRuntimeSprites();
        CreateLineMaterial();
        ApplyRuntimeAssets();
        CreateTransitionOverlay();

        CacheBasePositions();
        BuildSpeedLines();
        BuildGlitchWaves();

        SetPhase(1);
        initialized = true;

        StartCoroutine(RunOpeningSequence());
    }

    private void Update()
    {
        if (!initialized || !Application.isPlaying || uiRoot == null)
            return;

        Vector2 motion = GetGlobalMotionOffset();
        float rotation = GetGlobalMotionRotation();

        uiRoot.anchoredPosition = uiBasePos + motion;
        uiRoot.localRotation = Quaternion.Euler(0f, 0f, rotation);

        UpdateCubeVisuals();
        UpdateRiftLine();
        UpdateRealitySplit();
        UpdateSpeedLines(motion);
        UpdateGlitchWaves(motion);
        UpdateCinematicCamera();
    }

    private void OnDestroy()
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        CleanupRuntimeAssets();
    }

    private bool ValidateReferences()
    {
        bool ok = true;

        if (uiRoot == null) { Debug.LogError("OpeningCutsceneController: uiRoot is missing."); ok = false; }
        if (worldCamera == null) { Debug.LogError("OpeningCutsceneController: worldCamera is missing."); ok = false; }

        if (realityA == null) { Debug.LogError("OpeningCutsceneController: realityA is missing."); ok = false; }
        if (realityB == null) { Debug.LogError("OpeningCutsceneController: realityB is missing."); ok = false; }

        if (flashOverlay == null) { Debug.LogError("OpeningCutsceneController: flashOverlay is missing."); ok = false; }
        if (vignetteOverlay == null) { Debug.LogError("OpeningCutsceneController: vignetteOverlay is missing."); ok = false; }
        if (riftLine == null) { Debug.LogError("OpeningCutsceneController: riftLine is missing."); ok = false; }

        if (cube == null) { Debug.LogError("OpeningCutsceneController: cube is missing."); ok = false; }
        if (cubeShadowRed == null) { Debug.LogError("OpeningCutsceneController: cubeShadowRed is missing."); ok = false; }
        if (cubeShadowCyan == null) { Debug.LogError("OpeningCutsceneController: cubeShadowCyan is missing."); ok = false; }

        if (dialogueText == null) { Debug.LogError("OpeningCutsceneController: dialogueText is missing."); ok = false; }
        if (dialogueGroup == null) { Debug.LogError("OpeningCutsceneController: dialogueGroup is missing."); ok = false; }

        return ok;
    }

    private void CreateRuntimeTextures()
    {
        whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();

        int size = 256;
        vignetteTex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x / (size - 1f)) * 2f - 1f;
                float ny = (y / (size - 1f)) * 2f - 1f;
                float d = Mathf.Sqrt(nx * nx + ny * ny);
                float a = Mathf.Clamp01((d - 0.35f) / 0.65f);
                vignetteTex.SetPixel(x, y, new Color(0f, 0f, 0f, a * 0.85f));
            }
        }

        vignetteTex.Apply();
    }

    private void CreateRuntimeSprites()
    {
        whiteSprite = Sprite.Create(
            whiteTex,
            new Rect(0, 0, whiteTex.width, whiteTex.height),
            new Vector2(0.5f, 0.5f),
            100f
        );
    }

    private void CreateLineMaterial()
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            Debug.LogError("OpeningCutsceneController: Could not find 'Sprites/Default' shader.");
            return;
        }

        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    private void CreateTransitionOverlay()
    {
        if (transitionGroup != null && transitionImage != null)
            return;

        Canvas canvas = uiRoot != null ? uiRoot.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            Debug.LogWarning("OpeningCutsceneController: No Canvas found for transition overlay. Falling back to provided references only.");
            return;
        }

        GameObject go = new GameObject("RuntimeTransitionOverlay");
        go.transform.SetParent(canvas.transform, false);

        transitionImage = go.AddComponent<Image>();
        transitionImage.sprite = whiteSprite;
        transitionImage.type = Image.Type.Simple;
        transitionImage.preserveAspect = false;
        transitionImage.raycastTarget = true;
        transitionImage.color = Color.black;

        RectTransform rt = transitionImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;

        transitionGroup = go.AddComponent<CanvasGroup>();
        transitionGroup.alpha = 1f;
        transitionGroup.blocksRaycasts = true;
        transitionGroup.interactable = false;
    }

    private void ApplyRuntimeAssets()
    {
        if (flashOverlay != null)
        {
            flashOverlay.texture = whiteTex;
            SetRawImageAlpha(flashOverlay, 0f);
        }

        if (vignetteOverlay != null)
        {
            vignetteOverlay.texture = vignetteTex;
            vignetteOverlay.color = Color.white;
        }

        if (riftLine != null)
        {
            riftLine.texture = whiteTex;
            SetRawImageAlpha(riftLine, 0f);
        }

        if (cube != null)
        {
            cube.sprite = playerSprite != null ? playerSprite : whiteSprite;
            cube.type = Image.Type.Simple;
            cube.preserveAspect = true;
            cube.color = Color.white;
        }

        if (cubeShadowRed != null)
        {
            if (shadowSprite != null) cubeShadowRed.sprite = shadowSprite;
            else if (playerSprite != null) cubeShadowRed.sprite = playerSprite;
            else cubeShadowRed.sprite = whiteSprite;

            cubeShadowRed.type = Image.Type.Simple;
            cubeShadowRed.preserveAspect = true;
            cubeShadowRed.color = RedShadowDim;
        }

        if (cubeShadowCyan != null)
        {
            if (shadowSprite != null) cubeShadowCyan.sprite = shadowSprite;
            else if (playerSprite != null) cubeShadowCyan.sprite = playerSprite;
            else cubeShadowCyan.sprite = whiteSprite;

            cubeShadowCyan.type = Image.Type.Simple;
            cubeShadowCyan.preserveAspect = true;
            cubeShadowCyan.color = CyanShadowDim;
        }

        if (realityA != null)
            realityA.textureOverride = realityATexture;

        if (realityB != null)
            realityB.textureOverride = realityBTexture;

        if (dialogueGroup != null)
            dialogueGroup.alpha = 0f;
    }

    private void CleanupRuntimeAssets()
    {
        if (lineMaterial != null)
        {
            Destroy(lineMaterial);
            lineMaterial = null;
        }

        if (whiteSprite != null)
        {
            Destroy(whiteSprite);
            whiteSprite = null;
        }

        if (whiteTex != null)
        {
            Destroy(whiteTex);
            whiteTex = null;
        }

        if (vignetteTex != null)
        {
            Destroy(vignetteTex);
            vignetteTex = null;
        }
    }

    private void CacheBasePositions()
    {
        uiBasePos = uiRoot != null ? uiRoot.anchoredPosition : Vector2.zero;
        cubeBasePos = cube != null ? cube.rectTransform.anchoredPosition : Vector2.zero;
        redBasePos = cubeShadowRed != null ? cubeShadowRed.rectTransform.anchoredPosition : Vector2.zero;
        cyanBasePos = cubeShadowCyan != null ? cubeShadowCyan.rectTransform.anchoredPosition : Vector2.zero;
        riftBasePos = riftLine != null ? riftLine.rectTransform.anchoredPosition : Vector2.zero;

        if (worldCamera != null)
        {
            cameraBaseOrthoSize = worldCamera.orthographicSize;
            cameraBaseFov = worldCamera.fieldOfView;
        }
    }

    private void BuildSpeedLines()
    {
        if (worldCamera == null || lineMaterial == null)
            return;

        for (int i = 0; i < speedLineCount; i++)
        {
            GameObject go = new GameObject("SpeedLine_" + i);
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            InitializeLineRenderer(lr, 1, 2);

            SpeedLine s = new SpeedLine
            {
                lr = lr,
                seed = Random.Range(0f, 9999f)
            };

            ResetSpeedLine(s, true);
            speedLines.Add(s);
        }
    }

    private void BuildGlitchWaves()
    {
        if (worldCamera == null || lineMaterial == null)
            return;

        for (int i = 0; i < glitchWaveCount; i++)
        {
            GameObject go = new GameObject("GlitchWave_" + i);
            go.transform.SetParent(transform, false);

            LineRenderer lr = go.AddComponent<LineRenderer>();
            InitializeLineRenderer(lr, 2, 18);

            GlitchWave w = new GlitchWave
            {
                lr = lr,
                baseY = Random.Range(0f, Screen.height),
                thickness = Random.Range(1f, 5f),
                seed = Random.Range(0f, 9999f),
                color = Random.value > 0.5f
                    ? new Color(1f, 0f, 0.26f, 0.55f)
                    : new Color(0.08f, 0f, 0f, 0.8f)
            };

            glitchWaves.Add(w);
        }
    }

    private void InitializeLineRenderer(LineRenderer lr, int sortingOrder, int positionCount)
    {
        lr.sharedMaterial = lineMaterial;
        lr.positionCount = positionCount;
        lr.useWorldSpace = true;
        lr.numCapVertices = 0;
        lr.numCornerVertices = 0;
        lr.sortingOrder = sortingOrder;
        lr.enabled = false;
    }

    private IEnumerator RunOpeningSequence()
    {
        yield return StartCoroutine(FadeCanvasGroup(transitionGroup, 1f, 0f, introFadeFromBlackTime));
        if (transitionGroup != null)
            transitionGroup.blocksRaycasts = false;

        yield return StartCoroutine(Timeline());
    }

    private IEnumerator Timeline()
    {
        SetPhase(1);
        ShowDialogue("Systems nominal. Approaching coordinate grid...", PHASE1_DIALOGUE_TIME);

        yield return new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(3.5f);

        SetPhase(2);
        ShowDialogue("Warning: Spatial anomaly detected! I'm losing control!", PHASE2_DIALOGUE_TIME);

        yield return new WaitForSeconds(2.8f);

        SetPhase(3);
        ShowDialogue("Initiating emergency hyper-jump!", PHASE3_DIALOGUE_TIME);

        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(FlashRoutine());

        yield return new WaitForSeconds(0.8f);
        SetPhase(4);

        yield return new WaitForSeconds(1.7f);
        ShowDialogue("CRITICAL ERROR: Navigation failed. Where am I?", PHASE4_DIALOGUE_TIME);

        yield return new WaitForSeconds(3.2f);
        ShowDialogue("The coordinates are overlapping! I'm stuck between...", 3.5f);

        yield return new WaitForSeconds(4f);

        if (dialogueGroup != null)
            dialogueGroup.alpha = 0f;

        yield return StartCoroutine(ExitToMainMenuRoutine());
    }

    private IEnumerator ExitToMainMenuRoutine()
    {
        if (transitionGroup != null)
        {
            transitionGroup.blocksRaycasts = true;
            transitionGroup.interactable = false;
            yield return StartCoroutine(FadeCanvasGroup(transitionGroup, transitionGroup.alpha, 1f, exitFadeToBlackTime));
        }

        yield return new WaitForSeconds(finalHoldBeforeLoad);

        if (loadMainMenuSceneAtEnd && !string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else if (gameplayRoot != null)
        {
            gameplayRoot.SetActive(true);
        }

        Debug.Log("Cutscene complete.");
    }

    private void SetPhase(int newPhase)
    {
        phase = newPhase;

        if (cubeShadowRed != null)
            cubeShadowRed.gameObject.SetActive(phase == 2);

        if (cubeShadowCyan != null)
            cubeShadowCyan.gameObject.SetActive(phase == 2);

        if (riftLine != null)
            riftLine.gameObject.SetActive(phase == 4);

        if (realityA != null)
            realityA.gameObject.SetActive(phase == 4);

        if (realityB != null)
            realityB.gameObject.SetActive(phase == 4);

        if (phase == 4 && riftLine != null)
            SetRawImageAlpha(riftLine, 0.75f);
    }

    private void UpdateCubeVisuals()
    {
        if (cube == null)
            return;

        float t = Time.time;

        if (phase == 1)
        {
            Vector2 drift = new Vector2(
                Mathf.Sin(t * 0.8f) * 6f,
                Mathf.Sin(t * 1.05f) * 4f
            );

            cube.rectTransform.anchoredPosition = cubeBasePos + drift;
            cube.rectTransform.localScale = Vector3.one;
            cube.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 0.6f) * 0.4f);

            if (cubeShadowRed != null)
            {
                cubeShadowRed.color = RedShadowDim;
                cubeShadowRed.rectTransform.anchoredPosition = redBasePos;
            }

            if (cubeShadowCyan != null)
            {
                cubeShadowCyan.color = CyanShadowDim;
                cubeShadowCyan.rectTransform.anchoredPosition = cyanBasePos;
            }
        }
        else if (phase == 2)
        {
            Vector2 drift = new Vector2(
                Mathf.Sin(t * 16f) * 2.5f,
                Mathf.Cos(t * 14f) * 2f
            );

            cube.rectTransform.anchoredPosition = cubeBasePos + drift;
            cube.rectTransform.localScale = Vector3.one;
            cube.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 18f) * 1.2f);

            if (cubeShadowRed != null)
            {
                cubeShadowRed.color = new Color(RedShadowColor.r, RedShadowColor.g, RedShadowColor.b, 0.8f);
                cubeShadowRed.rectTransform.anchoredPosition = redBasePos + new Vector2(6f + Mathf.Sin(t * 13f) * 2f, 0f);
            }

            if (cubeShadowCyan != null)
            {
                cubeShadowCyan.color = new Color(CyanShadowColor.r, CyanShadowColor.g, CyanShadowColor.b, 0.8f);
                cubeShadowCyan.rectTransform.anchoredPosition = cyanBasePos + new Vector2(-6f + Mathf.Cos(t * 12f) * 2f, 0f);
            }
        }
        else if (phase == 3)
        {
            float pulse = Mathf.PingPong(t * 2f, 1f);
            float scale = Mathf.Lerp(1f, 1.75f, pulse);

            cube.rectTransform.anchoredPosition = cubeBasePos;
            cube.rectTransform.localScale = new Vector3(scale, scale, 1f);
            cube.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 22f) * 1.8f);

            if (cubeShadowRed != null)
            {
                cubeShadowRed.color = RedShadowDim;
                cubeShadowRed.rectTransform.anchoredPosition = redBasePos;
            }

            if (cubeShadowCyan != null)
            {
                cubeShadowCyan.color = CyanShadowDim;
                cubeShadowCyan.rectTransform.anchoredPosition = cyanBasePos;
            }
        }
        else if (phase == 4)
        {
            cube.rectTransform.anchoredPosition = cubeBasePos;
            cube.rectTransform.localScale = Vector3.one * 1.12f;
            cube.rectTransform.localRotation = Quaternion.identity;

            if (cubeShadowRed != null)
            {
                cubeShadowRed.color = RedShadowDim;
                cubeShadowRed.rectTransform.anchoredPosition = redBasePos;
            }

            if (cubeShadowCyan != null)
            {
                cubeShadowCyan.color = CyanShadowDim;
                cubeShadowCyan.rectTransform.anchoredPosition = cyanBasePos;
            }
        }
    }

    private void UpdateRiftLine()
    {
        if (riftLine == null)
            return;

        if (phase != 4)
        {
            SetRawImageAlpha(riftLine, 0f);
            return;
        }

        float t = Time.time;
        float alpha = 0.72f + Mathf.Sin(t * 1.2f) * 0.08f;

        riftLine.rectTransform.anchoredPosition = riftBasePos;
        riftLine.rectTransform.localRotation = Quaternion.identity;
        riftLine.rectTransform.localScale = Vector3.one;
        SetRawImageAlpha(riftLine, alpha);
    }

    private void UpdateRealitySplit()
    {
        if (realityA == null || realityB == null)
            return;

        if (phase != 4)
        {
            realityA.gameObject.SetActive(false);
            realityB.gameObject.SetActive(false);
            return;
        }

        realityA.gameObject.SetActive(true);
        realityB.gameObject.SetActive(true);

        float seam = 0.5f + Mathf.Sin(Time.time * 0.85f) * 0.003f;
        float left = seam - 0.016f;
        float right = seam + 0.016f;

        realityA.points = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(right, 1f),
            new Vector2(left, 0f)
        };

        realityB.points = new Vector2[4]
        {
            new Vector2(left, 0f),
            new Vector2(right, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        realityA.SetVerticesDirty();
        realityB.SetVerticesDirty();
    }

    private void UpdateSpeedLines(Vector2 motion)
    {
        if (phase != 1 && phase != 2)
        {
            for (int i = 0; i < speedLines.Count; i++)
            {
                if (speedLines[i].lr != null)
                    speedLines[i].lr.enabled = false;
            }

            return;
        }

        for (int i = 0; i < speedLines.Count; i++)
        {
            SpeedLine s = speedLines[i];
            if (s.lr == null) continue;

            s.lr.enabled = true;

            float phaseBoost = phase == 2 ? 1.8f : 1f;
            s.x -= s.speed * phaseBoost * Time.deltaTime * 60f;

            if (s.x < -s.length)
                ResetSpeedLine(s, false);

            float wobble = Mathf.Sin(Time.time * 8f + s.seed) * 1.5f;
            Vector2 startScreen = new Vector2(s.x + motion.x, s.y + motion.y + wobble);
            Vector2 endScreen = new Vector2(s.x + s.length + motion.x, s.y + motion.y + wobble);

            Vector3 p0 = ScreenToWorld(startScreen);
            Vector3 p1 = ScreenToWorld(endScreen);

            float width = PixelsToWorld(s.thickness);
            s.lr.startWidth = width;
            s.lr.endWidth = width;
            s.lr.startColor = s.color;
            s.lr.endColor = s.color;
            s.lr.SetPosition(0, p0);
            s.lr.SetPosition(1, p1);
        }
    }

    private void UpdateGlitchWaves(Vector2 motion)
    {
        if (phase != 2)
        {
            for (int i = 0; i < glitchWaves.Count; i++)
            {
                if (glitchWaves[i].lr != null)
                    glitchWaves[i].lr.enabled = false;
            }

            return;
        }

        for (int i = 0; i < glitchWaves.Count; i++)
        {
            GlitchWave w = glitchWaves[i];
            if (w.lr == null) continue;

            w.lr.enabled = true;
            w.baseY += Mathf.Sin(Time.time * 2.3f + w.seed) * 0.25f;

            if (w.baseY < 0f) w.baseY = Random.Range(0f, Screen.height);
            if (w.baseY > Screen.height) w.baseY = Random.Range(0f, Screen.height);

            int count = w.lr.positionCount;
            Vector3[] pts = new Vector3[count];

            for (int p = 0; p < count; p++)
            {
                float t = (count == 1) ? 0f : p / (count - 1f);
                float x = Mathf.Lerp(0f, Screen.width, t);

                float perlin = (Mathf.PerlinNoise(w.seed * 0.01f + t * 2f, Time.time * 1.5f) - 0.5f) * 64f;
                float wave = Mathf.Sin(Time.time * 20f + w.seed + p * 0.55f) * 18f;

                float y = w.baseY + perlin + wave + motion.y;
                x += motion.x;

                pts[p] = ScreenToWorld(new Vector2(x, y));
            }

            float width = PixelsToWorld(w.thickness);
            w.lr.startWidth = width;
            w.lr.endWidth = width;
            w.lr.startColor = w.color;
            w.lr.endColor = w.color;
            w.lr.SetPositions(pts);
        }
    }

    private void ResetSpeedLine(SpeedLine s, bool firstSpawn)
    {
        s.x = Random.Range(0f, Screen.width);
        s.y = Random.Range(0f, Screen.height);
        s.length = Random.Range(60f, 200f);
        s.speed = Random.Range(18f, 45f);
        s.thickness = Random.Range(0.6f, 2.2f);
        s.color = Random.value > 0.3f
            ? new Color(0f, 0.82f, 1f, 0.55f)
            : new Color(1f, 1f, 1f, 0.75f);

        if (!firstSpawn)
            s.x = Screen.width + s.length;
    }

    private void ShowDialogue(string text, float visibleTime)
    {
        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(DialogueRoutine(text, visibleTime));
    }

    private IEnumerator DialogueRoutine(string text, float visibleTime)
    {
        if (dialogueText != null)
            dialogueText.text = text;

        if (dialogueGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(dialogueGroup, dialogueGroup.alpha, 1f, FADE_IN_TIME));

        yield return new WaitForSeconds(visibleTime);

        if (dialogueGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(dialogueGroup, dialogueGroup.alpha, 0f, FADE_OUT_TIME));
    }

    private IEnumerator FlashRoutine()
    {
        if (flashOverlay == null)
            yield break;

        SetRawImageAlpha(flashOverlay, 1f);
        yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(FadeRawImageAlpha(flashOverlay, 1f, 0f, 1.1f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
            yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);
            n = n * n * (3f - 2f * n);
            group.alpha = Mathf.Lerp(from, to, n);
            yield return null;
        }

        group.alpha = to;
    }

    private IEnumerator FadeRawImageAlpha(RawImage img, float from, float to, float duration)
    {
        if (img == null)
            yield break;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / duration);
            n = n * n * (3f - 2f * n);
            SetRawImageAlpha(img, Mathf.Lerp(from, to, n));
            yield return null;
        }

        SetRawImageAlpha(img, to);
    }

    private void SetRawImageAlpha(RawImage img, float alpha)
    {
        if (img == null) return;

        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private void UpdateCinematicCamera()
    {
        if (worldCamera == null)
            return;

        float target = cameraBaseOrthoSize;

        if (phase == 1) target = cameraBaseOrthoSize * 1.015f;
        else if (phase == 2) target = cameraBaseOrthoSize * 1.045f;
        else if (phase == 3) target = cameraBaseOrthoSize * 1.085f;
        else if (phase == 4) target = cameraBaseOrthoSize * 1.03f;

        float smooth = 1f - Mathf.Exp(-Time.deltaTime * 3.5f);

        if (worldCamera.orthographic)
        {
            worldCamera.orthographicSize = Mathf.Lerp(worldCamera.orthographicSize, target, smooth);
        }
        else
        {
            float targetFov = cameraBaseFov;
            if (phase == 1) targetFov = cameraBaseFov * 0.985f;
            else if (phase == 2) targetFov = cameraBaseFov * 1.02f;
            else if (phase == 3) targetFov = cameraBaseFov * 1.04f;
            else if (phase == 4) targetFov = cameraBaseFov * 1.01f;

            worldCamera.fieldOfView = Mathf.Lerp(worldCamera.fieldOfView, targetFov, smooth);
        }
    }

    private Vector2 GetGlobalMotionOffset()
    {
        float t = Time.time;

        if (phase == 2)
            return new Vector2(Mathf.Sin(t * 21.5f) * 4f, Mathf.Cos(t * 17.8f) * 3f);

        if (phase == 3)
            return new Vector2(Mathf.Sin(t * 27f) * 1.5f, Mathf.Cos(t * 24f) * 1.5f);

        if (phase == 4)
            return new Vector2(Mathf.Sin(t * 6.5f) * 0.6f, Mathf.Cos(t * 7.2f) * 0.5f);

        return Vector2.zero;
    }

    private float GetGlobalMotionRotation()
    {
        float t = Time.time;

        if (phase == 2)
            return Mathf.Sin(t * 11.5f) * 0.45f;

        if (phase == 3)
            return Mathf.Sin(t * 14f) * 0.2f;

        if (phase == 4)
            return Mathf.Sin(t * 3.6f) * 0.08f;

        return 0f;
    }

    private Vector3 ScreenToWorld(Vector2 screenPos)
    {
        if (worldCamera == null)
            return Vector3.zero;

        float zDistance = Mathf.Abs(worldCamera.transform.position.z);
        return worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zDistance));
    }

    private float PixelsToWorld(float px)
    {
        if (worldCamera == null || !worldCamera.orthographic)
            return px * 0.01f;

        return (worldCamera.orthographicSize * 2f / Screen.height) * px;
    }
}