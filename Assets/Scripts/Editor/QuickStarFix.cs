using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public class QuickStarFix : EditorWindow
{
    [MenuItem("Tools/Star Rating System/Quick Fix Stars")]
    static void FixStars()
    {
        WinMenu winMenu = Object.FindFirstObjectByType<WinMenu>();
        if (winMenu == null)
        {
            Debug.LogError("No WinMenu found!");
            return;
        }

        GameObject winPanel = winMenu.winPanel;
        if (winPanel == null)
        {
            Debug.LogError("WinMenu has no Win Panel!");
            return;
        }

        StarAnimator animator = winPanel.GetComponentInChildren<StarAnimator>();
        
        if (animator == null)
        {
            GameObject container = new GameObject("Stars Container");
            container.transform.SetParent(winPanel.transform, false);
            
            RectTransform rect = container.AddComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(300, 100);
            
            animator = container.AddComponent<StarAnimator>();
            
            if (winMenu.starImages != null)
            {
                foreach (Image star in winMenu.starImages)
                {
                    if (star != null)
                    {
                        star.transform.SetParent(container.transform, true);
                    }
                }
            }
            
            animator.SetColors(winMenu.starFilledColor, winMenu.starEmptyColor);
            Debug.Log("Created Stars Container!");
        }
        
        if (winMenu.starAnimator == null)
        {
            winMenu.starAnimator = animator;
            EditorUtility.SetDirty(winMenu);
            Debug.Log("Connected StarAnimator!");
        }

        StarRatingSystem system = Object.FindFirstObjectByType<StarRatingSystem>();
        if (system == null)
        {
            GameObject obj = new GameObject("StarRatingSystem");
            obj.AddComponent<StarRatingSystem>();
            Debug.Log("Added StarRatingSystem!");
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("DONE! Press Play to test!");
    }
}
