using UnityEngine;
using UnityEditor;

public class AudioSetupHelper : EditorWindow
{
    [MenuItem("Tools/Audio/Check AudioManager Setup")]
    static void CheckSetup()
    {
        AudioManager manager = Object.FindFirstObjectByType<AudioManager>();
        
        string message = "🎵 AUDIO MANAGER CHECK:\n\n";
        
        if (manager == null)
        {
            message += "❌ AudioManager NOT FOUND!\n\n";
            message += "Create it:\n";
            message += "1. Create Empty GameObject\n";
            message += "2. Name it 'AudioManager'\n";
            message += "3. Add Component → AudioManager\n";
            message += "4. Add Component → Audio Source (x2)\n";
        }
        else
        {
            message += "✅ AudioManager found!\n\n";
            
            message += "SOUND EFFECTS:\n";
            message += (manager.jumpClip != null ? "✅" : "❌") + " Jump Sound\n";
            message += (manager.dieClip != null ? "✅" : "❌") + " Die Sound\n";
            message += (manager.switchClip != null ? "✅" : "❌") + " Switch Sound\n";
            message += (manager.winClip != null ? "✅" : "❌") + " Win Sound\n\n";
            
            message += "MUSIC:\n";
            message += (manager.menuMusicClip != null ? "✅" : "❌") + " Menu Music\n";
            message += (manager.gameMusicClip != null ? "✅" : "❌") + " Game Music\n\n";
            
            AudioSource[] sources = manager.GetComponents<AudioSource>();
            message += $"Audio Sources: {sources.Length} (need 2)\n\n";
            
            if (sources.Length < 2)
            {
                message += "⚠️ Add another Audio Source component!";
            }
            else
            {
                message += "✅ Ready to go!";
            }
        }
        
        Debug.Log(message);
        EditorUtility.DisplayDialog("Audio Setup Check", message, "OK");
    }
}
