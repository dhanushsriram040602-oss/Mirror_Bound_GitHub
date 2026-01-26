using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int levelReached = 1;

    private const string LEVEL_KEY = "LevelReached";
    private const string STAR_KEY_PREFIX = "Level_";
    private const string STAR_KEY_SUFFIX = "_Stars";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            levelReached = PlayerPrefs.GetInt(LEVEL_KEY, 1);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UnlockNextLevel(int completedLevel)
    {
        if (completedLevel >= levelReached)
        {
            levelReached = completedLevel + 1;
            PlayerPrefs.SetInt(LEVEL_KEY, levelReached);
            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int level)
    {
        return level <= levelReached;
    }

    public void SaveLevelStars(int levelIndex, int stars)
    {
        int currentStars = GetLevelStars(levelIndex);
        
        if (stars > currentStars)
        {
            string key = STAR_KEY_PREFIX + levelIndex + STAR_KEY_SUFFIX;
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
    }

    public int GetLevelStars(int levelIndex)
    {
        string key = STAR_KEY_PREFIX + levelIndex + STAR_KEY_SUFFIX;
        return PlayerPrefs.GetInt(key, 0);
    }

    public int GetTotalStars()
    {
        int total = 0;
        for (int i = 1; i <= levelReached; i++)
        {
            total += GetLevelStars(i);
        }
        return total;
    }

    public void ResetProgress()
    {
        levelReached = 1;
        PlayerPrefs.DeleteKey(LEVEL_KEY);
        
        for (int i = 1; i <= 100; i++)
        {
            string key = STAR_KEY_PREFIX + i + STAR_KEY_SUFFIX;
            PlayerPrefs.DeleteKey(key);
        }
        
        PlayerPrefs.Save();
    }
}