using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class CircularLevelSelector : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Total number of levels in your game")]
    public int totalLevels = 13;

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

    private Vector2 touchStartPos;
    private bool isDragging = false;
    private float dragStartOffset;

    [Header("Swipe Settings")]
    [Tooltip("How far must you swipe to go to next level? (pixels)")]
    public float swipeThreshold = 100f;

    void Start()
    {
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
            int levelIndex = i;

            GameObject btnObj = Instantiate(levelButtonPrefab, transform);

            Button btn = btnObj.GetComponentInChildren<Button>(true);
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (btn == null)
            {
                Debug.LogError("CircularLevelSelector: Button component not found in prefab!");
                Destroy(btnObj);
                continue;
            }

            bool isUnlocked = false;
            if (LevelManager.Instance != null)
            {
                isUnlocked = LevelManager.Instance.IsLevelUnlocked(levelIndex);
            }
            else
            {
                isUnlocked = (i == 1);
            }

            if (isUnlocked)
            {
                if (txt != null) txt.text = levelIndex.ToString();
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnLevelButtonClicked(levelIndex));
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
        // =====================================================
        // MOBILE TOUCH INPUT (Keep this for mobile/tablet)
        // =====================================================
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                isDragging = true;
                dragStartOffset = targetOffset;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float dragDistance = touch.position.x - touchStartPos.x;
                targetOffset = dragStartOffset + dragDistance;
                offsetVelocity = 0f;
            }
            else if (touch.phase == TouchPhase.Ended && isDragging)
            {
                isDragging = false;
                SnapToNearestLevel();
            }
        }

        // =====================================================
        // MOUSE INPUT FOR TESTING (Remove this section when building for mobile!)
        // START OF TESTING CODE - DELETE BEFORE MOBILE BUILD
        // =====================================================
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            isDragging = true;
            dragStartOffset = targetOffset;
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            float dragDistance = Input.mousePosition.x - touchStartPos.x;
            targetOffset = dragStartOffset + dragDistance;
            offsetVelocity = 0f;
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            SnapToNearestLevel();
        }
#endif
        // END OF TESTING CODE - DELETE ABOVE #if BLOCK BEFORE MOBILE BUILD
        // =====================================================
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

    void OnLevelButtonClicked(int levelIndex)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySwitch();

        if (levelIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(levelIndex);
        }
        else
        {
            Debug.LogError($"CircularLevelSelector: Scene index {levelIndex} is out of range!");
        }
    }

    public void SelectCenterLevel()
    {
        int centerLevel = selectedLevelIndex + 1;
        OnLevelButtonClicked(centerLevel);
    }
}
