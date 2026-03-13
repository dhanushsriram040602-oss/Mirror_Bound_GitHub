using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;

    [Header("Instruction Panel")]
    public GameObject instructionPanel;

    [Header("Other Panels")]
    public GameObject worldSelectPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;

    [Header("Settings")]
    public float inputDelayAfterClick = 0.1f;
    public bool playClickSound = true;

    [Header("Split Screen Background")]
    public SplitScreenTransition splitScreenTransition;

    private bool _waitingForInput;
    private const string LEVEL_SELECT_KEY = "OpenLevelSelect";

    void Start()
    {
        InitializeCursor();
        HandleStartupFlow();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMenuMusic();
    }

    void Update()
    {
        if (_waitingForInput)
            CheckForInput();
    }

    // ── Public navigation methods ─────────────────────────────────────────────

    public void ShowMainMenu()
    {
        _waitingForInput = false;
        HideAllPanels();

        if (splitScreenTransition != null)
            splitScreenTransition.ShowBackground();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    /// <summary>Used by the settings cross button to return to main menu with a click sound.</summary>
    public void SettingsCrossShowMainMenu()
    {
        _waitingForInput = false;
        HideAllPanels();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    public void ShowInstructions()
    {
        HideAllPanels();

        if (instructionPanel != null)
            instructionPanel.SetActive(true);

        Invoke(nameof(EnableInput), inputDelayAfterClick);
    }

    public void ShowWorldSelect()
    {
        _waitingForInput = false;
        HideAllPanels();

        if (worldSelectPanel != null)
            worldSelectPanel.SetActive(true);
    }

    public void ShowLevelSelect()
    {
        _waitingForInput = false;
        HideAllPanels();

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
        else
            SceneManager.LoadScene(1);
    }

    public void ShowSettingsPanel()
    {
        _waitingForInput = false;
        HideAllPanels();
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private void InitializeCursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HandleStartupFlow()
    {
        HideAllPanels();

        if (PlayerPrefs.GetInt(LEVEL_SELECT_KEY, 0) == 1)
        {
            PlayerPrefs.SetInt(LEVEL_SELECT_KEY, 0);
            PlayerPrefs.Save();
            ShowLevelSelect();
        }
        else
        {
            ShowMainMenu();
        }
    }

    /// <summary>
    /// Detects any tap, touch, or key press to advance from the instruction screen.
    /// Works on both mobile (Touchscreen) and PC (Keyboard / Mouse).
    /// </summary>
    private void CheckForInput()
    {
        bool tapped = false;

        // Touch (mobile / tablet)
        if (Touchscreen.current != null)
            tapped = Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

        // Mouse left click (editor / PC)
        if (!tapped && Mouse.current != null)
            tapped = Mouse.current.leftButton.wasPressedThisFrame;

        // Any keyboard key (PC)
        if (!tapped && Keyboard.current != null)
            tapped = Keyboard.current.anyKey.wasPressedThisFrame;

        if (tapped)
            ShowWorldSelect();
    }

    private void EnableInput()
    {
        _waitingForInput = true;
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel    != null) mainMenuPanel.SetActive(false);
        if (instructionPanel != null) instructionPanel.SetActive(false);
        if (worldSelectPanel != null) worldSelectPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (settingsPanel    != null) settingsPanel.SetActive(false);
    }
}