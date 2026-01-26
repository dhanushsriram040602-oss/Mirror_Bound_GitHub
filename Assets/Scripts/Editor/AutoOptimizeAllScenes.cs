using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public class AutoOptimizeAllScenes
{
    [MenuItem("Tools/Mobile/Optimize All Scenes for Touch")]
    static void OptimizeAll()
    {
        string[] scenePaths = new string[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_1.unity",
            "Assets/Scenes/Level_2.unity"
        };

        Scene originalScene = SceneManager.GetActiveScene();
        string originalScenePath = originalScene.path;

        int totalChanges = 0;

        foreach (string scenePath in scenePaths)
        {
            if (System.IO.File.Exists(scenePath))
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int changes = OptimizeCurrentScene();
                totalChanges += changes;
                
                if (changes > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[AutoOptimize] Optimized {scene.name}: {changes} changes");
                }
            }
        }

        if (!string.IsNullOrEmpty(originalScenePath))
        {
            EditorSceneManager.OpenScene(originalScenePath, OpenSceneMode.Single);
        }

        EditorUtility.DisplayDialog(
            "All Scenes Optimized!",
            $"✅ Made {totalChanges} total optimization(s) across all scenes\n\n" +
            "Optimizations:\n" +
            "• Drag threshold reduced to 5px\n" +
            "• Touch responsiveness boosted\n" +
            "• Multi-touch enabled\n" +
            "• 60 FPS target\n\n" +
            "Rebuild your game now!",
            "OK"
        );
    }

    static int OptimizeCurrentScene()
    {
        int changes = 0;

        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
        {
            eventSystem.sendNavigationEvents = false;

            InputSystemUIInputModule inputModule = eventSystem.GetComponent<InputSystemUIInputModule>();
            if (inputModule != null)
            {
                SerializedObject so = new SerializedObject(inputModule);
                
                SerializedProperty dragThreshold = so.FindProperty("m_DragThreshold");
                if (dragThreshold != null && dragThreshold.floatValue != 5)
                {
                    dragThreshold.floatValue = 5;
                    changes++;
                }

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(inputModule);
            }

            SerializedObject eventSO = new SerializedObject(eventSystem);
            SerializedProperty pixelDragThreshold = eventSO.FindProperty("m_PixelDragThreshold");
            if (pixelDragThreshold != null && pixelDragThreshold.intValue != 5)
            {
                pixelDragThreshold.intValue = 5;
                changes++;
            }
            eventSO.ApplyModifiedProperties();
            EditorUtility.SetDirty(eventSystem);
        }

        GameObject touchBoost = GameObject.Find("TouchResponsivenessBoost");
        if (touchBoost == null)
        {
            touchBoost = new GameObject("TouchResponsivenessBoost");
            touchBoost.AddComponent<TouchResponsivenessBoost>();
            changes++;
        }

        return changes;
    }
}
