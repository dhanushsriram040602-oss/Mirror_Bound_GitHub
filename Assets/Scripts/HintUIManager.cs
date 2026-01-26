using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HintUIManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject hintPanel;
    public TextMeshProUGUI hintText;
    public GameObject buyButtonObj;
    public GameObject closeButtonObj;
    public TextMeshProUGUI buyButtonText;

    [Header("Purchase Confirmation")]
    public GameObject purchasePanel;
    public TextMeshProUGUI purchaseText;

    [Header("Animation Settings")]
    public float animationDuration = 0.2f;

    private bool hintUnlockedForThisLevel = false;
    private const string HINT_PANEL_NAME = "HintPanel";
    private const string PURCHASE_PANEL_NAME = "PurchasePanel";

    void Awake()
    {
        AutoFindPanels();
    }

    void Start()
    {
        HideAllPanels();
    }

    private void AutoFindPanels()
    {
        if (hintPanel == null)
        {
            Transform t = transform.Find(HINT_PANEL_NAME);
            if (t != null)
            {
                hintPanel = t.gameObject;
            }
        }

        if (purchasePanel == null)
        {
            Transform t = transform.Find(PURCHASE_PANEL_NAME);
            if (t != null)
            {
                purchasePanel = t.gameObject;
            }
        }
    }

    private void HideAllPanels()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
        if (purchasePanel != null) purchasePanel.SetActive(false);
    }

    public void OpenHintMenu()
    {
        if (hintPanel != null)
        {
            StartCoroutine(AnimatePanelOpen(hintPanel));
            UpdateHintPanelState();
        }
    }

    private void UpdateHintPanelState()
    {
        if (GameManager.Instance == null)
        {
            if (hintText != null) hintText.text = "Error: GameManager missing.";
            return;
        }

        bool hasFreehints = GameManager.Instance.hintsUsed < GameManager.Instance.freeHintCount;

        if (hasFreehints || hintUnlockedForThisLevel)
        {
            ShowHintContent();
            if (buyButtonObj != null) buyButtonObj.SetActive(false);
        }
        else
        {
            if (hintText != null) hintText.text = "Hint Locked.";
            if (buyButtonObj != null)
            {
                buyButtonObj.SetActive(true);
                if (buyButtonText != null) buyButtonText.text = "Unlock Hint";
            }
        }
    }

    public void OnBuyButtonClicked()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.hintsUsed < GameManager.Instance.freeHintCount)
        {
            GameManager.Instance.hintsUsed++;
            hintUnlockedForThisLevel = true;
            UpdateHintPanelState();
        }
        else
        {
            OpenPurchaseConfirmation();
        }
    }

    private void OpenPurchaseConfirmation()
    {
        if (purchasePanel != null)
        {
            StartCoroutine(AnimatePanelOpen(purchasePanel));
            if (purchaseText != null)
            {
                purchaseText.text = $"Unlock hint for {GameManager.Instance.hintCost} coins?";
            }
        }
    }

    public void ConfirmPurchase()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.TryUseHint())
        {
            hintUnlockedForThisLevel = true;
            if (purchasePanel != null) StartCoroutine(AnimatePanelClose(purchasePanel));
            UpdateHintPanelState();
        }
        else
        {
            if (purchaseText != null) purchaseText.text = "Not enough coins!";
        }
    }

    public void CancelPurchase()
    {
        if (purchasePanel != null) StartCoroutine(AnimatePanelClose(purchasePanel));
    }

    private void ShowHintContent()
    {
        LevelSettings settings = FindFirstObjectByType<LevelSettings>();
        string message = "No hint available for this level.";

        if (settings != null)
        {
            message = settings.levelHint;
        }

        if (hintText != null) hintText.text = message;
    }

    public void CloseHint()
    {
        if (hintPanel != null && hintPanel.activeSelf)
        {
            StartCoroutine(AnimatePanelClose(hintPanel));
        }
        if (purchasePanel != null && purchasePanel.activeSelf)
        {
            StartCoroutine(AnimatePanelClose(purchasePanel));
        }
    }

    private IEnumerator AnimatePanelOpen(GameObject panel)
    {
        panel.SetActive(true);
        panel.transform.localScale = Vector3.zero;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animationDuration;
            float scale = Mathf.SmoothStep(0f, 1f, progress);
            panel.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        panel.transform.localScale = Vector3.one;
    }

    private IEnumerator AnimatePanelClose(GameObject panel)
    {
        float timer = 0f;
        Vector3 startScale = panel.transform.localScale;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;
            float progress = timer / animationDuration;
            float scale = Mathf.SmoothStep(1f, 0f, progress);
            panel.transform.localScale = startScale * scale;
            yield return null;
        }

        panel.transform.localScale = Vector3.zero;
        panel.SetActive(false);
    }
}