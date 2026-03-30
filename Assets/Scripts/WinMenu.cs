using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class WinMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject winPanel;
    
    [Header("Star Display")]
    public Image[] starImages;
    public Color starFilledColor = Color.yellow;
    public Color starEmptyColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Header("Star Animation")]
    public bool useStarAnimation = true;
    public StarAnimator starAnimator;
    
    [Header("Time Display")]
    public TextMeshProUGUI timeText;

    private const string LEVEL_SELECT_KEY = "OpenLevelSelect";

    private int starsEarned;
    private float completionTime;

    void Start()
    {
        if (starAnimator == null)
        {
            starAnimator = GetComponentInChildren<StarAnimator>();
        }

        if (starAnimator != null)
        {
            starAnimator.SetColors(starFilledColor, starEmptyColor);
        }
    }

    public void ShowWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (StarRatingSystem.Instance != null)
        {
            StarRatingSystem.Instance.EndLevel();
            completionTime = StarRatingSystem.Instance.GetCompletionTime();
            Debug.Log($"[WinMenu] Completion Time: {completionTime:F2}s");
        }
        else
        {
            completionTime = 0f;
            Debug.LogError("[WinMenu] StarRatingSystem not found! Add it to the scene.");
        }

        CalculateAndDisplayStars();
        DisplayCompletionTime();
        SaveProgress();

        Time.timeScale = 0f;
    }

    private void CalculateAndDisplayStars()
    {
        LevelSettings settings = FindFirstObjectByType<LevelSettings>();
        
        if (StarRatingSystem.Instance != null && settings != null)
        {
            starsEarned = StarRatingSystem.Instance.CalculateStars(settings);
            Debug.Log($"[WinMenu] Stars earned: {starsEarned}");
            
            if (useStarAnimation && starAnimator != null)
            {
                starAnimator.SetStars(starsEarned, true);
            }
            else
            {
                UpdateStarDisplay(starsEarned);
            }
        }
        else
        {
            if (StarRatingSystem.Instance == null)
            {
                Debug.LogError("[WinMenu] StarRatingSystem.Instance is NULL!");
            }
            if (settings == null)
            {
                Debug.LogError("[WinMenu] LevelSettings is NULL!");
            }
            
            starsEarned = 1;
            UpdateStarDisplay(starsEarned);
        }
    }

    private void DisplayCompletionTime()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {completionTime:F2}s";
        }
    }

    private void UpdateStarDisplay(int stars)
    {
        if (starImages == null || starImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < starImages.Length && i < 3; i++)
        {
            if (starImages[i] != null)
            {
                starImages[i].color = (i < stars) ? starFilledColor : starEmptyColor;
            }
        }
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;
        
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            LoadLevelSelect();
        }
    }

    public void LoadLevelSelect()
    {
        Time.timeScale = 1f;

        PlayerPrefs.SetInt(LEVEL_SELECT_KEY, 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(0);
    }

    private void SaveProgress()
    {
        int currentLevelIndex = SceneManager.GetActiveScene().buildIndex;
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.UnlockNextLevel(currentLevelIndex);
            LevelManager.Instance.SaveLevelStars(currentLevelIndex, starsEarned);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CommitLevelCoins();
        }
    }
}