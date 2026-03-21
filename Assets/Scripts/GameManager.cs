using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coinsCollected = 0;
    public int coinsCollectedThisLevel = 0;
    public int hintsUsed = 0;
    
    [Header("Settings")]
    public int hintCost = 5;
    public int freeHintCount = 2;

    private const string COINS_KEY = "TotalCoins";
    private const string HINTS_KEY = "HintsUsed";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadData()
    {
        coinsCollected = PlayerPrefs.GetInt(COINS_KEY, 0);
        hintsUsed = PlayerPrefs.GetInt(HINTS_KEY, 0);
    }

    public void AddCoin()
    {
        coinsCollectedThisLevel++;
        SaveData();
        UpdateUI();
    }

    public void SpendCoins(int amount)
    {
        if (coinsCollected >= amount)
        {
            coinsCollected -= amount;
            SaveData();
            UpdateUI();
        }
    }

    public bool TryUseHint()
    {
        if (hintsUsed < freeHintCount)
        {
            hintsUsed++;
            SaveData();
            return true;
        }

        if (coinsCollected >= hintCost)
        {
            SpendCoins(hintCost);
            hintsUsed++;
            SaveData();
            return true;
        }

        return false;
    }

    public void ResetLevelData()
    {
        coinsCollectedThisLevel = 0;
    }

    public void AddLevelCoinsToTotal()
    {
        coinsCollected += coinsCollectedThisLevel;
        SaveData();
        UpdateUI();
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(COINS_KEY, coinsCollected);
        PlayerPrefs.SetInt(HINTS_KEY, hintsUsed);
        PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        PlayerPrefs.SetInt(COINS_KEY, coinsCollected + coinsCollectedThisLevel);
        PlayerPrefs.Save();
    }

    private void UpdateUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(coinsCollected);
        }
    }
}