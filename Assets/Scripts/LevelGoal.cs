using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            HandleLevelComplete();
        }
    }

    private void HandleLevelComplete()
    {
        PlayWinSound();
        ShowWinScreen();
    }

    private void PlayWinSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWin();
        }
    }

    private void ShowWinScreen()
    {
        WinMenu winMenu = FindFirstObjectByType<WinMenu>();
        
        if (winMenu != null)
        {
            winMenu.ShowWin();
        }
    }
}