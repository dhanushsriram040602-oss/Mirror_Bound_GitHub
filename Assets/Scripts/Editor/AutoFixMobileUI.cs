using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public class AutoFixMobileUI
{
    static AutoFixMobileUI()
    {
        EditorApplication.delayCall += FixMainMenuUI;
    }

    static void FixMainMenuUI()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        
        if (activeScene.name != "MainMenu")
        {
            return;
        }

        bool madeChanges = false;

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas != null)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null && scaler.matchWidthOrHeight != 0.5f)
            {
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
                EditorUtility.SetDirty(scaler);
                madeChanges = true;
                Debug.Log("[AutoFix] Canvas Scaler updated: 1920x1080, Match = 0.5");
            }
        }

        RectTransform[] allRects = Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        foreach (RectTransform rect in allRects)
        {
            if (rect.name == "Back_Button_Border")
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(80, -50);
                
                EditorUtility.SetDirty(rect);
                madeChanges = true;
                Debug.Log($"[AutoFix] Fixed back button: {GetGameObjectPath(rect.gameObject)}");
            }
        }

        if (madeChanges)
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("<color=green>[AutoFix] ✅ Mobile UI fixed! Canvas and back buttons updated.</color>");
        }
    }

    static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    [MenuItem("Tools/UI/Fix Mobile UI Now")]
    static void ManualFix()
    {
        FixMainMenuUI();
        Debug.Log("[AutoFix] Manual fix completed!");
    }
}
