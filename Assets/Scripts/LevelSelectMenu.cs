using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject levelButtonPrefab;
    public Transform gridLayoutGroup;
    public int levelsPerWorld = 10;
    
    [Header("World Display (Optional)")]
    public TextMeshProUGUI worldTitleText;
    public TextMeshProUGUI totalStarsText;

    [Header("Star Colors")]
    public Color starFilledColor = Color.yellow;
    public Color starEmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Header("Star Animation")]
    public bool animateStars = true;

    private int currentWorld = 1;

    void Start()
    {
        if (gridLayoutGroup == null)
        {
            GridLayoutGroup layout = GetComponent<GridLayoutGroup>();
            if (layout != null)
            {
                gridLayoutGroup = transform;
            }
            else
            {
                Debug.LogError("LevelSelectMenu ERROR: No Grid Layout Group found!");
                return;
            }
        }

        if (WorldManager.Instance != null)
        {
            currentWorld = WorldManager.Instance.currentWorld;
        }

        UpdateWorldTitle();
        GenerateLevelButtons();
    }

    void UpdateWorldTitle()
    {
        if (worldTitleText != null)
        {
            worldTitleText.text = $"World {currentWorld}";
        }

        if (totalStarsText != null && LevelManager.Instance != null)
        {
            int totalStars = LevelManager.Instance.GetTotalStars();
            totalStarsText.text = $"Total Stars: {totalStars}";
        }
    }

    void GenerateLevelButtons()
    {
        if (gridLayoutGroup == null) return;

        foreach (Transform child in gridLayoutGroup)
        {
            if (child != null) Destroy(child.gameObject);
        }

        if (levelButtonPrefab == null)
        {
            Debug.LogError("LevelSelectMenu: Level Button Prefab missing!");
            return;
        }

        int startLevel = WorldManager.Instance != null 
            ? WorldManager.Instance.GetStartLevelForWorld(currentWorld)
            : 1;

        for (int i = 0; i < levelsPerWorld; i++)
        {
            int globalLevelIndex = startLevel + i;
            int displayNumber = i + 1;

            GameObject btnObj = Instantiate(levelButtonPrefab, gridLayoutGroup);
            Button btn = btnObj.GetComponentInChildren<Button>(true);
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>(true);

            if (btn == null || txt == null)
            {
                Debug.LogError("Button prefab missing components!");
                Destroy(btnObj);
                return;
            }

            bool isUnlocked = LevelManager.Instance != null
                ? LevelManager.Instance.IsLevelUnlocked(globalLevelIndex)
                : (i == 0);

            if (isUnlocked)
            {
                txt.text = displayNumber.ToString();
                btn.interactable = true;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => LoadLevel(globalLevelIndex));

                UpdateButtonStars(btnObj, globalLevelIndex);
            }
            else
            {
                txt.text = "";
                var colors = btn.colors;
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
                btn.colors = colors;
                btn.interactable = false;
            }
        }
    }

    void UpdateButtonStars(GameObject buttonObj, int levelIndex)
    {
        if (LevelManager.Instance == null) return;

        int stars = LevelManager.Instance.GetLevelStars(levelIndex);

        StarAnimator starAnimator = buttonObj.GetComponentInChildren<StarAnimator>();
        
        if (starAnimator != null)
        {
            starAnimator.SetColors(starFilledColor, starEmptyColor);
            starAnimator.SetStars(stars, animateStars);
        }
        else
        {
            Image[] starImages = buttonObj.GetComponentsInChildren<Image>(true);
            
            int starCount = 0;
            foreach (Image img in starImages)
            {
                if (img.gameObject.name.Contains("Star"))
                {
                    img.color = (starCount < stars) ? starFilledColor : starEmptyColor;
                    img.gameObject.SetActive(true);
                    starCount++;
                    
                    if (starCount >= 3) break;
                }
            }
            
            if (starCount == 0)
            {
                Debug.LogWarning($"[LevelSelectMenu] No stars found in button prefab! Make sure star images have 'Star' in their name.");
            }
        }
    }

    void LoadLevel(int index)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwitch();
            AudioManager.Instance.FadeOutMusic();
        }

        if (index < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(index);
        }
        else
        {
            Debug.LogError($"LevelSelectMenu: Scene index {index} is out of range! Add your levels to Build Settings.");
        }
    }
}