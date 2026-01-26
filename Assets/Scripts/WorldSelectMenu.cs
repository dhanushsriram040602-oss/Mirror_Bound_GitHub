using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldSelectMenu : MonoBehaviour
{
    [Header("World Buttons")]
    public Button world1Button;
    public Button world2Button;
    
    [Header("Button Text")]
    public TextMeshProUGUI world1Text;
    public TextMeshProUGUI world2Text;
    
    [Header("Lock Indicators (Optional)")]
    public GameObject world1Lock;
    public GameObject world2Lock;

    void Start()
    {
        SetupWorldButtons();
    }

    void SetupWorldButtons()
    {
        bool world2Unlocked = WorldManager.Instance != null && 
                              WorldManager.Instance.IsWorldUnlocked(2);

        if (world1Button != null)
        {
            world1Button.interactable = true;
            world1Button.onClick.RemoveAllListeners();
            world1Button.onClick.AddListener(() => SelectWorld(1));
        }

        if (world1Text != null)
            world1Text.text = "World 1";

        if (world1Lock != null)
            world1Lock.SetActive(false);

        if (world2Button != null)
        {
            world2Button.interactable = world2Unlocked;
            world2Button.onClick.RemoveAllListeners();
            world2Button.onClick.AddListener(() => SelectWorld(2));
            
            var colors = world2Button.colors;
            if (!world2Unlocked)
            {
                colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
            world2Button.colors = colors;
        }

        if (world2Text != null)
            world2Text.text = world2Unlocked ? "World 2" : "???";

        if (world2Lock != null)
            world2Lock.SetActive(!world2Unlocked);
    }

    void SelectWorld(int worldNumber)
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.SetWorld(worldNumber);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySwitch();
        }

        FindAnyObjectByType<MainMenu>()?.ShowLevelSelect();
    }
}
