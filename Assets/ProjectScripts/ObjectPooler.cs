using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single generic pooling system for the whole endless runner.
/// Every pooled prefab (platforms, coins, obstacles, environment props, power-ups...)
/// is registered under a "tag" (defaults to the prefab name) and fetched/returned by that tag.
/// Replaces PlatformPoolingSystem, CoinsPoolingSystem, EnvironmentPoolingSystem and ObstaclesPoolingSystem.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        [Min(1)] public int size = 10;
        public bool canExtend = true;
    }

    [Tooltip("Configure every prefab that will ever be pooled here (platform, coin, each obstacle, each environment prop, etc).")]
    public List<Pool> pools = new List<Pool>();

    private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, Pool> poolConfigByTag = new Dictionary<string, Pool>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        for (int i = 0; i < pools.Count; i++)
        {
            RegisterPool(pools[i]);
        }
    }

    private void RegisterPool(Pool pool)
    {
        if (pool == null || pool.prefab == null) return;

        if (string.IsNullOrEmpty(pool.tag))
        {
            pool.tag = pool.prefab.name;
        }

        if (poolDictionary.ContainsKey(pool.tag)) return;

        Queue<GameObject> queue = new Queue<GameObject>();
        for (int i = 0; i < pool.size; i++)
        {
            GameObject obj = Instantiate(pool.prefab, transform, true);
            obj.SetActive(false);
            queue.Enqueue(obj);
        }

        poolDictionary.Add(pool.tag, queue);
        poolConfigByTag.Add(pool.tag, pool);
    }

    /// <summary>
    /// Registers a pool at runtime for a prefab that wasn't set up in the inspector
    /// (e.g. a new power-up/collectible unlocked by difficulty). No-op if a pool for
    /// that prefab's name already exists.
    /// </summary>
    public void EnsurePoolExists(GameObject prefab, int size = 5, bool canExtend = true)
    {
        if (prefab == null) return;
        if (poolDictionary.ContainsKey(prefab.name)) return;

        Pool pool = new Pool { tag = prefab.name, prefab = prefab, size = size, canExtend = canExtend };
        pools.Add(pool);
        RegisterPool(pool);
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"ObjectPooler: no pool registered for tag '{tag}'.");
            return null;
        }

        if (queue.Count > 0)
        {
            GameObject obj = queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        if (poolConfigByTag.TryGetValue(tag, out Pool config) && config.canExtend)
        {
            GameObject newObj = Instantiate(config.prefab, transform, true);
            newObj.SetActive(true);
            return newObj;
        }

        Debug.LogWarning($"ObjectPooler: pool '{tag}' is empty and cannot extend.");
        return null;
    }

    /// <summary>
    /// Convenience overload for callers that hold a prefab reference instead of a string tag
    /// (this is what the Coins/Obstacles/Environment logic uses). Auto-registers the pool
    /// on first use if it wasn't pre-configured in the inspector.
    /// </summary>
    public GameObject GetPooledObject(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!poolDictionary.ContainsKey(prefab.name))
        {
            EnsurePoolExists(prefab);
        }

        return GetPooledObject(prefab.name);
    }

    public void ReturnObject(GameObject obj, string tag)
    {
        if (obj == null) return;

        if (!poolDictionary.TryGetValue(tag, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"ObjectPooler: tried to return an object with unknown tag '{tag}'. Destroying it instead.");
            Destroy(obj);
            return;
        }

        obj.transform.SetParent(transform);
        obj.SetActive(false);
        queue.Enqueue(obj);
    }

    public void ReturnObject(GameObject obj, GameObject prefab)
    {
        if (prefab == null || obj == null) return;
        ReturnObject(obj, prefab.name);
    }

    public bool HasPool(string tag) => poolDictionary.ContainsKey(tag);
}