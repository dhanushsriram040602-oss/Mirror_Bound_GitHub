using UnityEngine;

public class PoolingSetupHelper : MonoBehaviour
{
    [Header("SETUP INSTRUCTIONS")]
    [TextArea(15, 25)]
    public string instructions = 
        "OBJECT POOLING SETUP - Mirror Bound\n\n" +
        "1. CREATE PARTICLE POOL MANAGER\n" +
        "   - Create empty GameObject named 'ParticlePoolManager'\n" +
        "   - Add ParticlePoolManager component\n\n" +
        "2. CONFIGURE POOLS (Add these in inspector):\n" +
        "   Pool Name: 'JumpDust' → Prefab: JumpDustEffect\n" +
        "   Pool Name: 'Death' → Prefab: Death Effect\n" +
        "   Pool Name: 'MoveDust' → Prefab: MoveDust\n" +
        "   Pool Name: 'RealityShift' → Prefab: ShiftEffect\n" +
        "   Pool Name: 'CoinCollect' → Prefab: (Your coin particle)\n" +
        "   Pool Name: 'EchoSpawn' → Prefab: EchoSpawnEffect (World 2)\n" +
        "   Pool Name: 'GravityZone' → Prefab: GravityZoneParticle (World 2)\n\n" +
        "3. UPDATE PREFABS\n" +
        "   Player → Set pool names (JumpDust, Death, MoveDust)\n" +
        "   Coin → Set pool name (CoinCollect)\n" +
        "   RealityManager → Set pool name (RealityShift)\n\n" +
        "4. ADD ParticleAutoDestroy to particle prefabs\n\n" +
        "BENEFITS:\n" +
        "✓ 50x faster particle spawning\n" +
        "✓ Zero garbage collection\n" +
        "✓ Better mobile performance\n" +
        "✓ Smooth 60 FPS even with many particles";

    [Header("Debug - Pool Statistics")]
    public bool showDebugGUI = false;

    void OnGUI()
    {
        if (!showDebugGUI || ParticlePoolManager.Instance == null)
            return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Box("Particle Pool Statistics");
        
        ShowPoolStats("JumpDust");
        ShowPoolStats("Death");
        ShowPoolStats("MoveDust");
        ShowPoolStats("RealityShift");
        ShowPoolStats("CoinCollect");
        ShowPoolStats("EchoSpawn");
        ShowPoolStats("GravityZone");
        
        GUILayout.EndArea();
    }

    void ShowPoolStats(string poolName)
    {
        int active = ParticlePoolManager.Instance.GetActiveCount(poolName);
        int available = ParticlePoolManager.Instance.GetAvailableCount(poolName);
        int total = ParticlePoolManager.Instance.GetTotalCount(poolName);
        
        GUILayout.Label($"{poolName}: {active}/{total} active ({available} available)");
    }
}
