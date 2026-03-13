using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class CircularLevelSelector : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Number of levels per world (must match WorldManager.LEVELS_PER_WORLD)")]
    public int totalLevels = 10;

    [Header("Button Prefab")]
    [Tooltip("Drag your Level Button prefab here")]
    public GameObject levelButtonPrefab;

    [Header("Horizontal Carousel Settings")]
    [Tooltip("Space between each level button (Recommended: 150-250)")]
    public float spacing = 200f;

    [Tooltip("Vertical position offset (moves carousel up/down)")]
    public float verticalOffset = 0f;

    [Header("Scroll Settings")]
    [Tooltip("How smoothly does it move? (Lower = smoother, 0.1-0.5 recommended)")]
    public float smoothTime = 0.3f;

    [Header("Center Focus Effect")]
    [Tooltip("Should the centered level be bigger?")]
    public bool scaleCenter = true;

    [Tooltip("How much bigger is the center? (1 = normal, 2 = double size)")]
    public float centerScale = 1.5f;

    [Tooltip("How small are the far buttons? (Recommended: 0.5-0.8)")]
    public float minScale = 0.6f;

    [Tooltip("Should far levels fade out?")]
    public bool fadeNonCenter = true;

    [Tooltip("Minimum opacity for far buttons (0 = invisible, 1 = full)")]
    public float minAlpha = 0.3f;

    [Header("3D Depth Effect (Optional)")]
    [Tooltip("Add depth perspective effect?")]
    public bool use3DEffect = true;

    [Tooltip("How much depth? (0 = flat, 100 = more depth)")]
    public float depthAmount = 50f;

    private List<GameObject> levelButtons = new List<GameObject>();
    private float currentOffset = 0f;
    private float targetOffset = 0f;
    private float offsetVelocity = 0f;
    private int selectedLevelIndex = 0;

    // World-aware level tracking
    private int currentWorld = 1;
    private int worldStartGlobalLevel = 1;

    private Vector2 touchStartPos;
    private bool isDragging = false;
    private float dragStartOffset;

    [Header("Swipe Settings")]
    [Tooltip("How far must you swipe to go to next level? (pixels)")]
    public float swipeThreshold = 100f;

    void Start()
    {
        // Resolve which world is active and where its levels start globally
        if (WorldManager.Instance != null)
        {
            currentWorld           = WorldManager.Instance.currentWorld;
            worldStartGlobalLevel  = WorldManager.LEVELS_PER_WORLD * (currentWorld - 1) + 1;
        }

        CreateLevelButtons();
        selectedLevelIndex = 0;
        targetOffset = 0f;
        UpdateButtonPositions();
    }

    void CreateLevelButtons()
    {
        if (levelButtonPrefab == null)
        {
            Debug.LogError("CircularLevelSelector: Please assign a Level Button Prefab in the Inspector!");
            return;
        }

        for (int i = 1; i <= totalLevels; i++)
        {
            int localLevel  = i;
            int globalLevel = worldStartGlobalLevel + i - 1;

            GameObject btnObj = Instantiate(levelButtonPrefab, transform);

            Button btn = btnObj.GetComponentInChildren<Button>(true);
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (btn == null)
            {
                Debug.LogError("CircularLevelSelector: Button component not found in prefab!");
                Destroy(btnObj);
                continue;
            }

            bool isUnlocked = LevelManager.Instance != null
                ? LevelManager.Instance.IsLevelUnlocked(globalLevel)
                : (i == 1);

            if (isUnlocked)
            {
                if (txt != null) txt.text = localLevel.ToString();
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnLevelButtonClicked(localLevel));
            }
            else
            {
                if (txt != null) txt.text = "🔒";
                btn.interactable = false;
            }

            levelButtons.Add(btnObj);
        }
    }

    void Update()
    {
        HandleTouchInput();

        currentOffset = Mathf.SmoothDamp(currentOffset, targetOffset, ref offsetVelocity, smoothTime);

        UpdateButtonPositions();
    }

    void HandleTouchInput()
    {
        // ── Mobile touch (new Input System) ──────────────────────────────────
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                touchStartPos  = touch.position.ReadValue();
                isDragging     = true;
                dragStartOffset = targetOffset;
            }
            else if (touch.press.isPressed && isDragging)
            {
                float dragDistance = touch.position.ReadValue().x - touchStartPos.x;
                targetOffset  = dragStartOffset + dragDistance;
                offsetVelocity = 0f;
            }
            else if (touch.press.wasReleasedThisFrame && isDragging)
            {
                isDragging = false;
                SnapToNearestLevel();
            }

            return; // Touchscreen present — skip mouse fallback
        }

        // ── Editor / PC mouse fallback ────────────────────────────────────────
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Mouse.current != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                touchStartPos   = mousePos;
                isDragging      = true;
                dragStartOffset = targetOffset;
            }
            else if (Mouse.current.leftButton.isPressed && isDragging)
            {
                float dragDistance = mousePos.x - touchStartPos.x;
                targetOffset   = dragStartOffset + dragDistance;
                offsetVelocity = 0f;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
            {
                isDragging = false;
                SnapToNearestLevel();
            }
        }
#endif
    }

    void SnapToNearestLevel()
    {
        float swipeDelta = targetOffset - dragStartOffset;

        if (Mathf.Abs(swipeDelta) > swipeThreshold)
        {
            if (swipeDelta > 0)
            {
                ScrollLeft();
            }
            else
            {
                ScrollRight();
            }
        }
        else
        {
            targetOffset = -selectedLevelIndex * spacing;
        }
    }

    public void ScrollLeft()
    {
        selectedLevelIndex--;
        if (selectedLevelIndex < 0) selectedLevelIndex = totalLevels - 1;

        targetOffset = -selectedLevelIndex * spacing;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySwitch();
    }

    public void ScrollRight()
    {
        selectedLevelIndex++;
        if (selectedLevelIndex >= totalLevels) selectedLevelIndex = 0;

        targetOffset = -selectedLevelIndex * spacing;

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySwitch();
    }

    void UpdateButtonPositions()
    {
        for (int i = 0; i < levelButtons.Count; i++)
        {
            float xPos = (i * spacing) + currentOffset;
            float yPos = verticalOffset;

            float distanceFromCenter = Mathf.Abs(xPos);
            float normalizedDistance = Mathf.Clamp01(distanceFromCenter / spacing);

            if (use3DEffect)
            {
                float depthNormalized = Mathf.Clamp01(distanceFromCenter / (spacing * 2f));
                yPos += -depthNormalized * depthAmount;
            }

            levelButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, yPos);

            if (scaleCenter)
            {
                float scale = Mathf.Lerp(centerScale, minScale, normalizedDistance);
                levelButtons[i].transform.localScale = Vector3.one * scale;
            }

            if (fadeNonCenter)
            {
                CanvasGroup canvasGroup = levelButtons[i].GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = levelButtons[i].AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = Mathf.Lerp(1f, minAlpha, normalizedDistance);
            }

            int siblingIndex = Mathf.RoundToInt((1f - normalizedDistance) * 1000);
            levelButtons[i].transform.SetSiblingIndex(siblingIndex);
        }
    }

    void OnLevelButtonClicked(int localLevel)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySwitch();

        string sceneName = BuildSceneName(currentWorld, localLevel);

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"CircularLevelSelector: Scene '{sceneName}' not found in Build Settings!");
        }
    }

    /// <summary>
    /// Constructs the scene name for a given world and local level (1–10).
    /// World 1: "Level_1" … "Level_10"
    /// World 2+: "World {world}_Level_{level}"
    /// </summary>
    private string BuildSceneName(int world, int level)
    {
        return world == 1 ? $"Level_{level}" : $"World {world}_Level_{level}";
    }

    public void SelectCenterLevel()
    {
        OnLevelButtonClicked(selectedLevelIndex + 1);
    }
}
