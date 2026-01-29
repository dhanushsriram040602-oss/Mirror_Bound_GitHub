using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoad]
public class ProjectInputOptimizer
{
    static ProjectInputOptimizer()
    {
        EditorApplication.delayCall += OptimizeProjectSettings;
    }

    static void OptimizeProjectSettings()
    {
        bool madeChanges = false;

        if (PlayerSettings.Android.targetSdkVersion != AndroidSdkVersions.AndroidApiLevelAuto)
        {
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            madeChanges = true;
        }

#if UNITY_2023_1_OR_NEWER
        var androidTarget = NamedBuildTarget.Android;

        if (!PlayerSettings.GetMobileMTRendering(androidTarget))
        {
            PlayerSettings.SetMobileMTRendering(androidTarget, true);
            madeChanges = true;
        }
#endif


        if (madeChanges)
        {
            Debug.Log("<color=cyan>[InputOptimizer] ✅ Project settings optimized for mobile touch</color>");
        }
    }

    [MenuItem("Tools/Mobile/Fix Input Handling Warning")]
    static void FixInputHandling()
    {
        #if ENABLE_INPUT_SYSTEM && ENABLE_LEGACY_INPUT_MANAGER
        EditorUtility.DisplayDialog(
            "Input Handling Warning",
            "Your project is using BOTH Input Systems (new + old).\n\n" +
            "This causes performance issues on Android.\n\n" +
            "To fix:\n" +
            "1. Go to Edit → Project Settings → Player\n" +
            "2. Find 'Active Input Handling'\n" +
            "3. Change from 'Both' to 'Input System Package (New)'\n" +
            "4. Restart Unity\n\n" +
            "This will improve touch response!",
            "OK"
        );
        #else
        EditorUtility.DisplayDialog(
            "Input Handling OK",
            "✅ Your input handling is correctly configured!\n\nUsing: Input System Package (New)",
            "OK"
        );
        #endif

        SettingsService.OpenProjectSettings("Project/Player");
    }
}
