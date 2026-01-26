using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class FixBackButtonAnchors : EditorWindow
{
    [MenuItem("Tools/UI/Fix Back Button Anchors")]
    static void Fix()
    {
        RectTransform[] allRects = Object.FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        int fixedCount = 0;

        foreach (RectTransform rect in allRects)
        {
            if (rect.name.Contains("Back_Button") || rect.name.Contains("Back Button"))
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0.5f, 0.5f);
                
                Vector2 currentPos = rect.anchoredPosition;
                rect.anchoredPosition = new Vector2(
                    Mathf.Max(60, currentPos.x),
                    Mathf.Min(-30, currentPos.y)
                );

                EditorUtility.SetDirty(rect);
                fixedCount++;
                Debug.Log($"Fixed: {rect.name} at position {rect.anchoredPosition}");
            }
        }

        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog(
                "Back Buttons Fixed",
                $"Fixed {fixedCount} back button(s)!\n\n" +
                "Anchors set to top-left corner.\n" +
                "Position adjusted to be visible on all screens.\n\n" +
                "Test in Game view with different resolutions!",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "No Back Buttons Found",
                "Could not find any GameObjects with 'Back_Button' or 'Back Button' in their name.\n\n" +
                "Make sure your back buttons are in the scene!",
                "OK"
            );
        }
    }
}
