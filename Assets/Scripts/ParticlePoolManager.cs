using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    [System.Serializable]
    public class ParticlePoolConfig
    {
        public string poolName;
        public GameObject particlePrefab;
        public int initialSize = 10;
        public bool expandable = true;
    }

    [Header("Particle Pool Configuration")]
    public List<ParticlePoolConfig> particlePools = new List<ParticlePoolConfig>();

    [Header("Auto-Return Settings")]
    public bool autoReturnParticles = false;

    private Dictionary<string, ObjectPool<ParticleSystem>> pools;
    private Transform poolParent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllCoroutines();
    }

    private void InitializePools()
    {
        poolParent = new GameObject("ParticlePools").transform;
        poolParent.SetParent(transform);
        
        pools = new Dictionary<string, ObjectPool<ParticleSystem>>();

        foreach (var config in particlePools)
        {
            if (config.particlePrefab == null)
            {
                Debug.LogWarning($"Particle prefab for pool '{config.poolName}' is null!");
                continue;
            }

            ParticleSystem ps = config.particlePrefab.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                Debug.LogWarning($"Prefab '{config.particlePrefab.name}' doesn't have a ParticleSystem component!");
                continue;
            }

            Transform poolContainer = new GameObject($"Pool_{config.poolName}").transform;
            poolContainer.SetParent(poolParent);

            ObjectPool<ParticleSystem> pool = new ObjectPool<ParticleSystem>(
                ps,
                config.initialSize,
                config.expandable,
                poolContainer
            );

            pools.Add(config.poolName, pool);
        }
    }

    public ParticleSystem SpawnParticle(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Particle pool '{poolName}' not found!");
            return null;
        }

        ParticleSystem ps = pools[poolName].Get(position, rotation);
        
        if (ps != null)
        {
            ps.Play();

            if (autoReturnParticles)
            {
                StartCoroutine(ReturnAfterPlay(poolName, ps));
            }
        }

        return ps;
    }

    public ParticleSystem SpawnParticle(string poolName, Vector3 position)
    {
        return SpawnParticle(poolName, position, Quaternion.identity);
    }

    public void ReturnParticle(string poolName, ParticleSystem ps)
    {
        if (pools.ContainsKey(poolName))
        {
            pools[poolName].Return(ps);
        }
    }

    public void ReturnAllParticles(string poolName)
    {
        if (pools.ContainsKey(poolName))
        {
            pools[poolName].ReturnAll();
        }
    }

    public void ReturnAllParticles()
    {
        foreach (var pool in pools.Values)
        {
            pool.ReturnAll();
        }
    }

    private IEnumerator ReturnAfterPlay(string poolName, ParticleSystem ps)
    {
        if (ps == null) yield break;

        while (true)
        {
            if (ps == null) yield break;
            
            bool isActive = false;
            bool isPlaying = false;

            try
            {
                isActive = ps.gameObject.activeInHierarchy;
                if (!isActive)
                {
                    ReturnParticle(poolName, ps);
                    yield break;
                }

                isPlaying = ps.isPlaying;
                if (!isPlaying)
                {
                    yield break;
                }
            }
            catch
            {
                yield break;
            }

            yield return null;
        }
    }

    public int GetActiveCount(string poolName)
    {
        return pools.ContainsKey(poolName) ? pools[poolName].CountActive : 0;
    }

    public int GetAvailableCount(string poolName)
    {
        return pools.ContainsKey(poolName) ? pools[poolName].CountAvailable : 0;
    }

    public int GetTotalCount(string poolName)
    {
        return pools.ContainsKey(poolName) ? pools[poolName].CountTotal : 0;
    }
}
