using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public int currentWorld = 1;
    public const int LEVELS_PER_WORLD = 10;

    private const string WORLD_KEY = "CurrentWorld";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadCurrentWorld();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetWorld(int worldNumber)
    {
        currentWorld = worldNumber;
        PlayerPrefs.SetInt(WORLD_KEY, worldNumber);
        PlayerPrefs.Save();
    }

    public int GetStartLevelForWorld(int worldNumber)
    {
        return ((worldNumber - 1) * LEVELS_PER_WORLD) + 1;
    }

    public int GetWorldFromLevel(int levelIndex)
    {
        return ((levelIndex - 1) / LEVELS_PER_WORLD) + 1;
    }

    public bool IsWorldUnlocked(int worldNumber)
    {
        if (worldNumber == 1) return true;
        
        int previousWorldLastLevel = (worldNumber - 1) * LEVELS_PER_WORLD;
        
        if (LevelManager.Instance != null)
        {
            return LevelManager.Instance.IsLevelUnlocked(previousWorldLastLevel + 1);
        }
        
        return false;
    }

    private void LoadCurrentWorld()
    {
        currentWorld = PlayerPrefs.GetInt(WORLD_KEY, 1);
    }
}
