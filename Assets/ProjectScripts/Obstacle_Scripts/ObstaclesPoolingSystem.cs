using System.Collections.Generic;
using UnityEngine;

public class ObstaclesPoolingSystem : MonoBehaviour
{
    public List<GameObject> obstaclesPrefabs = new List<GameObject>();
    public int amountPerPrefab = 10;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (amountPerPrefab < 1)
        {
            amountPerPrefab = 1;
        }

        foreach (GameObject prefab in obstaclesPrefabs)
        {
            if (prefab == null || pools.ContainsKey(prefab)) continue;

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < amountPerPrefab; i++)
            {
                GameObject obstacleObject = Instantiate(prefab, transform, true);
                obstacleObject.SetActive(false);
                pool.Enqueue(obstacleObject);
            }

            pools.Add(prefab, pool);
        }
    }

    public GameObject GetObstacle(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        if (pool.Count > 0)
        {
            GameObject obstacleObject = pool.Dequeue();
            obstacleObject.SetActive(true);
            return obstacleObject;
        }

        GameObject newObstacleObject = Instantiate(prefab, transform, true);
        newObstacleObject.SetActive(true);
        return newObstacleObject;
    }

    public void ReturnObstacle(GameObject prefab, GameObject obstacleObject)
    {
        if (prefab == null || obstacleObject == null) return;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        obstacleObject.transform.SetParent(transform);
        obstacleObject.SetActive(false);
        pool.Enqueue(obstacleObject);
    }
}
