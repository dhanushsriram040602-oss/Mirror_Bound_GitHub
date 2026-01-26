using UnityEngine;

public class StarRatingSystem : MonoBehaviour
{
    public static StarRatingSystem Instance;

    private float levelStartTime;
    private float levelEndTime;
    private bool levelActive;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        levelStartTime = Time.time;
        levelEndTime = 0f;
        levelActive = true;
        Debug.Log($"[StarRatingSystem] Level started at {levelStartTime}");
    }

    public int CalculateStars(LevelSettings settings)
    {
        if (settings == null)
        {
            Debug.LogWarning("[StarRatingSystem] LevelSettings is null!");
            return 1;
        }

        float completionTime = GetCompletionTime();
        
        Debug.Log($"[StarRatingSystem] Completion Time: {completionTime:F2}s | 3★ Threshold: {settings.threeStarTime}s | 2★ Threshold: {settings.twoStarTime}s");

        if (completionTime <= settings.threeStarTime)
        {
            Debug.Log("[StarRatingSystem] Earned 3 STARS!");
            return 3;
        }
        else if (completionTime <= settings.twoStarTime)
        {
            Debug.Log("[StarRatingSystem] Earned 2 STARS!");
            return 2;
        }
        else
        {
            Debug.Log("[StarRatingSystem] Earned 1 STAR!");
            return 1;
        }
    }

    public float GetCurrentLevelTime()
    {
        if (levelActive)
        {
            return Time.time - levelStartTime;
        }
        return GetCompletionTime();
    }

    public float GetCompletionTime()
    {
        if (levelEndTime > 0f)
        {
            return levelEndTime - levelStartTime;
        }
        else if (levelActive)
        {
            return Time.time - levelStartTime;
        }
        return 0f;
    }

    public void EndLevel()
    {
        if (levelActive)
        {
            levelEndTime = Time.time;
            levelActive = false;
            Debug.Log($"[StarRatingSystem] Level ended at {levelEndTime}. Total time: {GetCompletionTime():F2}s");
        }
    }
}
