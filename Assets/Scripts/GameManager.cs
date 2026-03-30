using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // Persistent wallet — survives across all scenes.
    public int coinsCollected = 0;

    // In-level counter — reset on death and cleared on level complete.
    public int coinsCollectedThisLevel = 0;

    [Header("Settings")]
    public int hintCost = 5;
    public int freeHintCount = 2;

    private int hintsUsed = 0;

    /// <summary>Read-only access to the number of hints already used.</summary>
    public int HintsUsed => hintsUsed;

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

    /// <summary>
    /// Called by each Coin on collection. Increments both the in-level
    /// counter and the persistent wallet immediately, then refreshes the HUD.
    /// </summary>
    public void AddCoin()
    {
        coinsCollectedThisLevel++;
        coinsCollected++;
        SaveData();
        UpdateHUD();
    }

    /// <summary>
    /// Rolls back coins collected during the current failed attempt and
    /// removes them from the persistent wallet. Called on player death so
    /// only completed levels contribute coins permanently.
    /// </summary>
    public void ResetLevelCoins()
    {
        coinsCollected -= coinsCollectedThisLevel;
        coinsCollectedThisLevel = 0;
        coinsCollected = Mathf.Max(0, coinsCollected);
        SaveData();
        UpdateHUD();
    }

    /// <summary>
    /// Locks in coins from the completed level. The wallet was already
    /// updated in real-time by AddCoin, so this just resets the
    /// in-level counter without touching the persistent wallet.
    /// </summary>
    public void CommitLevelCoins()
    {
        coinsCollectedThisLevel = 0;
        SaveData();
    }

    /// <summary>Spends coins from the persistent wallet.</summary>
    public void SpendCoins(int amount)
    {
        if (coinsCollected >= amount)
        {
            coinsCollected -= amount;
            SaveData();
            UpdateHUD();
        }
    }

    /// <summary>
    /// Attempts to use a hint — free hints first, then paid with coins.
    /// Returns true if the hint was granted.
    /// </summary>
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

    private void SaveData()
    {
        PlayerPrefs.SetInt(COINS_KEY, coinsCollected);
        PlayerPrefs.SetInt(HINTS_KEY, hintsUsed);
        PlayerPrefs.Save();
    }

    private void UpdateHUD()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoinUI(coinsCollectedThisLevel, coinsCollected);
        }
    }
}