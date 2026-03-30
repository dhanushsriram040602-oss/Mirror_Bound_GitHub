using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI References")]
    public TextMeshProUGUI levelText;
    public Image realityBorderImage;
    public TextMeshProUGUI realityText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;

    [Header("Reality A Style (Warm)")]
    public Color colorA = new Color(1f, 0.5f, 0f);

    [Header("Reality B Style (Cool)")]
    public Color colorB = new Color(0f, 0.9f, 1f);

    private const string COIN_TEXT_NAME = "CoinCountText";
    private const string COIN_ALT_NAME = "CoinText";
    private const string SCORE_TEXT_NAME = "ScoreText";
    private const string TIMER_TEXT_NAME = "TimerText";

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CacheCoinTextReference();
        CacheTimerTextReference();
        UpdateLevelText();
        SubscribeToReality();
        UpdateInitialCoinUI();
    }

    void Update()
    {
        UpdateTimerDisplay();
    }

    void OnDestroy()
    {
        UnsubscribeFromReality();
    }

    private void CacheCoinTextReference()
    {
        if (coinText == null)
        {
            GameObject foundObj = GameObject.Find(COIN_TEXT_NAME);
            if (foundObj == null) foundObj = GameObject.Find(COIN_ALT_NAME);
            if (foundObj == null) foundObj = GameObject.Find(SCORE_TEXT_NAME);

            if (foundObj != null)
            {
                coinText = foundObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void CacheTimerTextReference()
    {
        if (timerText == null)
        {
            GameObject foundObj = GameObject.Find(TIMER_TEXT_NAME);
            if (foundObj != null)
            {
                timerText = foundObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null && StarRatingSystem.Instance != null)
        {
            float currentTime = StarRatingSystem.Instance.GetCurrentLevelTime();
            timerText.text = $"Time: {currentTime:F1}s";
        }
    }

    private void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = SceneManager.GetActiveScene().name.Replace("_", " ").ToUpper();
        }
    }

    private void SubscribeToReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange += UpdateRealityUI;
            UpdateRealityUI(RealityManager.Instance.currentReality);
        }
    }

    private void UnsubscribeFromReality()
    {
        if (RealityManager.Instance != null)
        {
            RealityManager.Instance.OnRealityChange -= UpdateRealityUI;
        }
    }

    private void UpdateInitialCoinUI()
    {
        if (GameManager.Instance != null)
        {
            UpdateCoinUI(
                GameManager.Instance.coinsCollectedThisLevel,
                GameManager.Instance.coinsCollected
            );
        }
    }

    private void UpdateRealityUI(RealityManager.Reality reality)
    {
        if (realityText == null) return;

        if (reality == RealityManager.Reality.A)
        {
            realityText.text = "REALITY A";
            realityText.color = colorA;

            if (realityBorderImage != null)
            {
                realityBorderImage.color = colorA;
            }
        }
        else
        {
            realityText.text = "REALITY B";
            realityText.color = colorB;

            if (realityBorderImage != null)
            {
                realityBorderImage.color = colorB;
            }
        }
    }

    /// <summary>
    /// Refreshes the coin HUD. Shows coins collected this level (in-level
    /// progress) alongside the persistent wallet total.
    /// </summary>
    public void UpdateCoinUI(int levelCoins, int totalCoins)
    {
        if (coinText != null)
        {
            coinText.text = $"{levelCoins}  ({totalCoins})";
        }
    }
}