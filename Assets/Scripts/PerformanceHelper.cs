using UnityEngine;

public class PerformanceHelper : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Set to 60 for smooth gameplay on most devices")]
    public int targetFrameRate = 60;
    
    [Header("Display FPS (Optional)")]
    public bool showFPS = false;
    public Color fpsColor = Color.green;
    
    float deltaTime = 0.0f;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = 0;
        
        Debug.Log($"Performance Helper: Target FPS set to {targetFrameRate}");
    }

    void Update()
    {
        if (showFPS)
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    }

    void OnGUI()
    {
        if (!showFPS) return;

        int w = Screen.width;
        int h = Screen.height;

        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(10, 10, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 50;
        style.normal.textColor = fpsColor;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);

        GUI.Label(rect, text, style);
    }
}
