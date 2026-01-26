using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu UI")]
    public GameObject mainMenuPanel;

    [Header("Instruction Panels")]
    public GameObject instructionPanelPC;
    public GameObject instructionPanelMobile;

    [Header("Other Panels")]
    public GameObject worldSelectPanel;
    public GameObject levelSelectPanel;

    [Header("Settings")]
    public float inputDelayAfterClick = 0.1f;

    private bool waitingForInput;
    private const string LEVEL_SELECT_KEY = "OpenLevelSelect";

    void Start()
    {
        InitializeCursor();
        HandleStartupFlow();
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    void Update()
    {
        if (waitingForInput)
        {
            CheckForInput();
        }
    }

    public void ShowMainMenu()
    {
        waitingForInput = false;
        HideAllPanels();

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
    }

    public void ShowInstructions()
    {
        HideAllPanels();

        if (Application.isMobilePlatform)
        {
            if (instructionPanelMobile != null)
            {
                instructionPanelMobile.SetActive(true);
            }
        }
        else
        {
            if (instructionPanelPC != null)
            {
                instructionPanelPC.SetActive(true);
            }
        }

        Invoke(nameof(EnableInput), inputDelayAfterClick);
    }

    public void ShowWorldSelect()
    {
        waitingForInput = false;
        HideAllPanels();

        if (worldSelectPanel != null)
        {
            worldSelectPanel.SetActive(true);
        }
    }

    public void ShowLevelSelect()
    {
        waitingForInput = false;
        HideAllPanels();

        if (levelSelectPanel != null)
        {
            levelSelectPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

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

    private void CheckForInput()
    {
        if (!Application.isMobilePlatform)
        {
            if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                ShowWorldSelect();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                ShowWorldSelect();
            }
        }
    }

    private void EnableInput()
    {
        waitingForInput = true;
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (instructionPanelPC != null) instructionPanelPC.SetActive(false);
        if (instructionPanelMobile != null) instructionPanelMobile.SetActive(false);
        if (worldSelectPanel != null) worldSelectPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }
}