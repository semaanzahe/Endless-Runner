using System.Collections.Generic;
using UnityEngine;

public class EnvironmentPoolingSystem : MonoBehaviour
{
    public List<GameObject> farObjectsPrefabs = new List<GameObject>();
    public List<GameObject> middleObjectsPrefabs = new List<GameObject>();
    public int amountPerPrefab = 10;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (amountPerPrefab < 1)
        {
            amountPerPrefab = 1;
        }

        PrewarmPrefabs(farObjectsPrefabs);
        PrewarmPrefabs(middleObjectsPrefabs);
    }

    public GameObject GetEnvironmentObject(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        if (pool.Count > 0)
        {
            GameObject environmentObject = pool.Dequeue();
            environmentObject.SetActive(true);
            return environmentObject;
        }

        GameObject newEnvironmentObject = Instantiate(prefab, transform, true);
        newEnvironmentObject.SetActive(true);
        return newEnvironmentObject;
    }

    public void ReturnEnvironmentObject(GameObject prefab, GameObject environmentObject)
    {
        if (prefab == null || environmentObject == null) return;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        environmentObject.transform.SetParent(transform);
        environmentObject.SetActive(false);
        pool.Enqueue(environmentObject);
    }

    private void PrewarmPrefabs(List<GameObject> prefabs)
    {
        if (prefabs == null) return;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab == null || pools.ContainsKey(prefab)) continue;

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < amountPerPrefab; i++)
            {
                GameObject environmentObject = Instantiate(prefab, transform, true);
                environmentObject.SetActive(false);
                pool.Enqueue(environmentObject);
            }

            pools.Add(prefab, pool);
        }
    }
}
