using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class DebugLevelButton : EditorWindow
{
    [MenuItem("Tools/Star Rating System/Debug Level Button")]
    static void CheckLevelButton()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Level Button.prefab");
        
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find Level Button.prefab at Assets/Prefabs/", "OK");
            return;
        }

        Image[] images = prefab.GetComponentsInChildren<Image>(true);
        StarAnimator animator = prefab.GetComponentInChildren<StarAnimator>(true);
        
        string message = "LEVEL BUTTON PREFAB CHECK:\n\n";
        
        message += $"Total Image components: {images.Length}\n\n";
        
        int starCount = 0;
        foreach (Image img in images)
        {
            if (img.gameObject.name.Contains("Star"))
            {
                starCount++;
                message += $"• {img.gameObject.name} (Active: {img.gameObject.activeSelf})\n";
            }
        }
        
        message += $"\nStars found: {starCount}\n";
        message += $"StarAnimator: {(animator != null ? "YES" : "NO")}\n\n";
        
        if (starCount == 0)
        {
            message += "⚠️ NO STARS FOUND!\n";
            message += "Your button needs 3 star images with 'Star' in the name.";
        }
        else if (starCount < 3)
        {
            message += "⚠️ Only found " + starCount + " stars. Need 3!";
        }
        else
        {
            message += "✅ Stars are set up correctly!";
        }
        
        Debug.Log(message);
        EditorUtility.DisplayDialog("Level Button Check", message, "OK");
    }
}
