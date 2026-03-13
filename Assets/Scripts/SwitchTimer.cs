using UnityEngine;
using TMPro;
using System.Collections;

public class SwitchTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float countdownTime = 7f;

    [Header("References")]
    public TextMeshProUGUI timerText;
    public GoldPlatform targetPlatform;

    private float currentTime;
    private bool isRunning = false;

    void Awake()
    {
        gameObject.SetActive(false); // hide at start
    }

    public void StartTimer()
    {
        if (isRunning) return;

        isRunning = true;
        currentTime = countdownTime;
        gameObject.SetActive(true);
        StartCoroutine(TimerRoutine());
    }

    IEnumerator TimerRoutine()
    {
        while (currentTime > 0f)
        {
            if (timerText != null)
                timerText.text = Mathf.Ceil(currentTime).ToString();

            currentTime -= Time.deltaTime;
            yield return null;
        }

        if (timerText != null)
            timerText.text = "0";

        if (targetPlatform != null)
            targetPlatform.Activate();

        gameObject.SetActive(false);
    }
}
