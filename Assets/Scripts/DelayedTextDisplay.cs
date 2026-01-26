using UnityEngine;

public class DelayedTextDisplay : MonoBehaviour
{
    [SerializeField] private float delayBeforeFadeIn = 2f;
    [SerializeField] private float fadeInDuration = 1f;
    
    private CanvasGroup canvasGroup;
    private float timer;
    private bool hasStartedFading;
    private bool isComplete;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0f;
        timer = 0f;
        hasStartedFading = false;
        isComplete = false;
    }
    
    void Update()
    {
        if (isComplete) return;
        
        timer += Time.deltaTime;
        
        if (timer >= delayBeforeFadeIn && !hasStartedFading)
        {
            hasStartedFading = true;
        }
        
        if (hasStartedFading)
        {
            float fadeProgress = (timer - delayBeforeFadeIn) / fadeInDuration;
            canvasGroup.alpha = Mathf.Clamp01(fadeProgress);
            
            if (fadeProgress >= 1f)
            {
                isComplete = true;
            }
        }
    }
}
