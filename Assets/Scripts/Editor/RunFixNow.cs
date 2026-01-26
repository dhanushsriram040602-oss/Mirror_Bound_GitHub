using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class RunFixNow
{
    [InitializeOnLoadMethod]
    static void RunFix()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.referenceResolution.x == 800)
            {
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
                Debug.Log("<color=cyan>✅ Canvas Scaler fixed: 1920x1080, Match=0.5</color>");

                RectTransform[] allRects = Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
                int count = 0;
                foreach (RectTransform rect in allRects)
                {
                    if (rect.name == "Back_Button_Border")
                    {
                        rect.anchoredPosition = new Vector2(80, -50);
                        rect.pivot = new Vector2(0.5f, 0.5f);
                        EditorUtility.SetDirty(rect);
                        count++;
                    }
                }

                if (count > 0)
                {
                    Debug.Log($"<color=green>✅ Fixed {count} back button(s) - Now at position (80, -50)</color>");
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
        };
    }
}
