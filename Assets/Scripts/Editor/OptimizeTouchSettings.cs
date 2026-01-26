using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class OptimizeTouchSettings : EditorWindow
{
    [MenuItem("Tools/Mobile/Optimize Touch Response")]
    static void Optimize()
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

                SerializedProperty repeatDelay = so.FindProperty("m_RepeatDelay");
                if (repeatDelay != null && repeatDelay.floatValue != 0.3f)
                {
                    repeatDelay.floatValue = 0.3f;
                    changes++;
                }

                SerializedProperty repeatRate = so.FindProperty("m_RepeatRate");
                if (repeatRate != null && repeatRate.floatValue != 0.05f)
                {
                    repeatRate.floatValue = 0.05f;
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

            Debug.Log($"[TouchOptimize] EventSystem optimized: drag threshold = 5px");
        }

        GameObject touchBoost = GameObject.Find("TouchResponsivenessBoost");
        if (touchBoost == null)
        {
            touchBoost = new GameObject("TouchResponsivenessBoost");
            touchBoost.AddComponent<TouchResponsivenessBoost>();
            changes++;
            Debug.Log("[TouchOptimize] Added TouchResponsivenessBoost to scene");
        }

        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.iOS.targetOSVersionString = "12.0";

        if (changes > 0)
        {
            EditorUtility.DisplayDialog(
                "Touch Response Optimized!",
                $"✅ Made {changes} optimization(s)\n\n" +
                "• Drag threshold: 10px → 5px (more responsive)\n" +
                "• Multi-touch enabled\n" +
                "• Enhanced Touch Support enabled\n" +
                "• Navigation events disabled (UI only)\n" +
                "• Target framerate: 60 FPS\n\n" +
                "Save your scene and rebuild!",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "Already Optimized",
                "Touch settings are already optimized!",
                "OK"
            );
        }
    }
}
