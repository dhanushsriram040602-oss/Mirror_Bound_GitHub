using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class StarSystemSetupHelper : EditorWindow
{
    [MenuItem("Tools/Star Rating System/Setup Guide")]
    static void ShowSetupGuide()
    {
        StarSystemSetupHelper window = GetWindow<StarSystemSetupHelper>("Star System Setup");
        window.minSize = new Vector2(500, 600);
        window.Show();
    }

    [MenuItem("Tools/Star Rating System/Add to Current Scene")]
    static void AddToCurrentScene()
    {
        GameObject starSystem = new GameObject("StarRatingSystem");
        starSystem.AddComponent<StarRatingSystem>();
        
        Selection.activeGameObject = starSystem;
        EditorGUIUtility.PingObject(starSystem);
        
        Debug.Log("StarRatingSystem added to scene! Configure LevelSettings next.");
    }

    [MenuItem("Tools/Star Rating System/Count Coins in Scene")]
    static void CountCoins()
    {
        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        Debug.Log($"Total Coins in {SceneManager.GetActiveScene().name}: {coins.Length}");
    }

    private Vector2 scrollPos;

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("⭐ Star Rating System Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        DrawSection("Quick Actions", () =>
        {
            if (GUILayout.Button("Add StarRatingSystem to Scene", GUILayout.Height(30)))
            {
                AddToCurrentScene();
            }

            if (GUILayout.Button("Count Coins in Scene", GUILayout.Height(30)))
            {
                CountCoins();
            }

            if (GUILayout.Button("Reset All Progress (PlayerPrefs)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Progress", 
                    "This will delete ALL saved stars and level progress. Continue?", 
                    "Yes", "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                    Debug.Log("All PlayerPrefs cleared!");
                }
            }
        });

        DrawSection("Step 1: Scene Setup", () =>
        {
            GUILayout.Label("For EACH gameplay level:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("1. Add StarRatingSystem GameObject (use button above)");
            GUILayout.Label("2. Configure LevelSettings with star thresholds");
            GUILayout.Label("3. Set total coins (use Count Coins button)");
        });

        DrawSection("Step 2: LevelSettings Configuration", () =>
        {
            GUILayout.Label("Required fields on LevelSettings:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Three Star Time: Complete under this (e.g., 15s)");
            GUILayout.Label("• Two Star Time: Complete under this (e.g., 30s)");
        });

        DrawSection("Step 3: Win Panel UI", () =>
        {
            GUILayout.Label("Add to WinMenu GameObject:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• 3 Image objects named Star1, Star2, Star3");
            GUILayout.Label("• Assign to 'Star Images' array in WinMenu");
            GUILayout.Label("• Set Star Filled Color (yellow) and Empty (gray)");
        });

        DrawSection("Step 4: Level Button Prefab", () =>
        {
            GUILayout.Label("Update Level Button prefab:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Add 3 Image children");
            GUILayout.Label("• Name them with 'Star' in the name");
            GUILayout.Label("• Position below level number");
            GUILayout.Label("• Script finds them by name automatically!");
        });

        DrawSection("Step 5: LevelSelectMenu", () =>
        {
            GUILayout.Label("Configure LevelSelectMenu:", EditorStyles.wordWrappedLabel);
            GUILayout.Label("• Set Star Filled Color and Empty Color");
            GUILayout.Label("• Optional: Add 'Total Stars Text' label");
        });

        DrawSection("Recommended Time Thresholds", () =>
        {
            GUILayout.Label("Easy levels:  3★=15s  2★=30s", EditorStyles.miniLabel);
            GUILayout.Label("Medium:       3★=30s  2★=60s", EditorStyles.miniLabel);
            GUILayout.Label("Hard:         3★=45s  2★=90s", EditorStyles.miniLabel);
            GUILayout.Space(5);
            GUILayout.Label("Tip: Play each level 2-3 times to find good times!", 
                EditorStyles.wordWrappedMiniLabel);
        });

        DrawSection("Testing", () =>
        {
            GUILayout.Label("✓ Complete a level and check stars display");
            GUILayout.Label("✓ Return to level select - stars should show");
            GUILayout.Label("✓ Replay and beat time - 'NEW BEST' should appear");
            GUILayout.Label("✓ Exit play mode - stars should persist");
        });

        DrawSection("Troubleshooting", () =>
        {
            if (GUILayout.Button("Check Current Scene Setup"))
            {
                CheckSceneSetup();
            }
        });

        GUILayout.Space(20);
        GUILayout.Label("Star Rating System v1.0", EditorStyles.centeredGreyMiniLabel);

        EditorGUILayout.EndScrollView();
    }

    void DrawSection(string title, System.Action content)
    {
        GUILayout.Space(10);
        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label(title, EditorStyles.boldLabel);
        GUILayout.Space(5);
        content();
        GUILayout.EndVertical();
    }

    void CheckSceneSetup()
    {
        Debug.Log("=== Star System Scene Check ===");

        StarRatingSystem starSystem = FindFirstObjectByType<StarRatingSystem>();
        if (starSystem != null)
        {
            Debug.Log("✓ StarRatingSystem found");
        }
        else
        {
            Debug.LogWarning("✗ StarRatingSystem NOT found - add it!");
        }

        LevelSettings settings = FindFirstObjectByType<LevelSettings>();
        if (settings != null)
        {
            Debug.Log("✓ LevelSettings found");
            Debug.Log($"  - 3 Star Time: {settings.threeStarTime}s");
            Debug.Log($"  - 2 Star Time: {settings.twoStarTime}s");
        }
        else
        {
            Debug.LogWarning("✗ LevelSettings NOT found");
        }

        Coin[] coins = FindObjectsByType<Coin>(FindObjectsSortMode.None);
        Debug.Log($"Coins in scene: {coins.Length}");
    }
}
