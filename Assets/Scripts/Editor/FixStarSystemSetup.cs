using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class FixStarSystemSetup : EditorWindow
{
    [MenuItem("Tools/Star Rating System/Fix Current Scene Setup")]
    static void FixCurrentScene()
    {
        bool wasFixed = false;
        
        WinMenu winMenu = FindFirstObjectByType<WinMenu>();
        if (winMenu == null)
        {
            EditorUtility.DisplayDialog("Error", "No WinMenu found in the current scene!", "OK");
            return;
        }

        GameObject winPanel = winMenu.winPanel;
        if (winPanel == null)
        {
            EditorUtility.DisplayDialog("Error", "WinMenu has no Win Panel assigned!", "OK");
            return;
        }

        StarAnimator starAnimator = winPanel.GetComponentInChildren<StarAnimator>();
        
        if (starAnimator == null)
        {
            GameObject starContainer = new GameObject("Stars Container");
            starContainer.transform.SetParent(winPanel.transform, false);
            
            RectTransform rectTransform = starContainer.AddComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(300, 100);
            
            starAnimator = starContainer.AddComponent<StarAnimator>();
            
            if (winMenu.starImages != null && winMenu.starImages.Length > 0)
            {
                foreach (Image starImage in winMenu.starImages)
                {
                    if (starImage != null)
                    {
                        starImage.transform.SetParent(starContainer.transform, true);
                    }
                }
            }
            
            starAnimator.SetColors(winMenu.starFilledColor, winMenu.starEmptyColor);
            
            Debug.Log("Created Stars Container with StarAnimator!");
            wasFixed = true;
        }
        
        if (winMenu.starAnimator == null)
        {
            winMenu.starAnimator = starAnimator;
            EditorUtility.SetDirty(winMenu);
            Debug.Log("Connected StarAnimator to WinMenu!");
            wasFixed = true;
        }

        StarRatingSystem starSystem = FindFirstObjectByType<StarRatingSystem>();
        if (starSystem == null)
        {
            GameObject starSystemObj = new GameObject("StarRatingSystem");
            starSystemObj.AddComponent<StarRatingSystem>();
            Debug.Log("Added StarRatingSystem to scene!");
            wasFixed = true;
        }

        LevelSettings settings = FindFirstObjectByType<LevelSettings>();
        if (settings != null)
        {
            if (settings.threeStarTime == 0 && settings.twoStarTime == 0)
            {
                settings.threeStarTime = 30f;
                settings.twoStarTime = 60f;
                EditorUtility.SetDirty(settings);
                Debug.Log("Set default time thresholds: 30s / 60s");
                wasFixed = true;
            }
        }

        if (wasFixed)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorUtility.DisplayDialog("Success!", 
                "Star system setup complete!\n\n" +
                "Stars Container created\n" +
                "StarAnimator connected\n" +
                "Stars moved to container\n\n" +
                "Press PLAY to test!", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Info", "Everything is already set up correctly!", "OK");
        }
    }

    [MenuItem("Tools/Star Rating System/Fix All Level Scenes")]
    static void FixAllScenes()
    {
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
        int fixedCount = 0;

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            
            if (!sceneName.StartsWith("Level") && !sceneName.Contains("World"))
                continue;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            bool sceneFixed = false;
            
            WinMenu winMenu = FindFirstObjectByType<WinMenu>();
            if (winMenu != null && winMenu.winPanel != null)
            {
                StarAnimator starAnimator = winMenu.winPanel.GetComponentInChildren<StarAnimator>();
                
                if (starAnimator == null)
                {
                    GameObject starContainer = new GameObject("Stars Container");
                    starContainer.transform.SetParent(winMenu.winPanel.transform, false);
                    
                    RectTransform rectTransform = starContainer.AddComponent<RectTransform>();
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = new Vector2(300, 100);
                    
                    starAnimator = starContainer.AddComponent<StarAnimator>();
                    
                    if (winMenu.starImages != null && winMenu.starImages.Length > 0)
                    {
                        foreach (Image starImage in winMenu.starImages)
                        {
                            if (starImage != null)
                            {
                                starImage.transform.SetParent(starContainer.transform, true);
                            }
                        }
                    }
                    
                    starAnimator.SetColors(winMenu.starFilledColor, winMenu.starEmptyColor);
                    sceneFixed = true;
                }
                
                if (winMenu.starAnimator == null)
                {
                    winMenu.starAnimator = starAnimator;
                    EditorUtility.SetDirty(winMenu);
                    sceneFixed = true;
                }
            }

            StarRatingSystem starSystem = FindFirstObjectByType<StarRatingSystem>();
            if (starSystem == null)
            {
                GameObject starSystemObj = new GameObject("StarRatingSystem");
                starSystemObj.AddComponent<StarRatingSystem>();
                sceneFixed = true;
            }

            if (sceneFixed)
            {
                EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                fixedCount++;
                Debug.Log($"✅ Fixed: {sceneName}");
            }
        }

        EditorUtility.DisplayDialog("Complete!", 
            $"Fixed {fixedCount} level scenes!\n\n" +
            "All levels now have:\n" +
            "✅ StarAnimator\n" +
            "✅ StarRatingSystem\n" +
            "✅ Proper connections", 
            "OK");
    }
}
