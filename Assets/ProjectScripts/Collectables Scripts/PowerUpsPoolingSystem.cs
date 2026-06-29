using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]

public class PowerUpsPoolingSystem : MonoBehaviour
{
    public List<GameObject> powerUpsPrefabs = new List<GameObject>();
    [Min(1)] public int amountPerPrefab = 10;

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private readonly Dictionary<GameObject, GameObject> activePowerUps = new Dictionary<GameObject, GameObject>();

    private void Awake()
    {
        if (amountPerPrefab < 1)
        {
            amountPerPrefab = 1;
        }

        EnsureFallbackPrefab();

        foreach (GameObject prefab in powerUpsPrefabs)
        {
            if (prefab == null || pools.ContainsKey(prefab)) continue;

            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < amountPerPrefab; i++)
            {
                GameObject powerUpObject = Instantiate(prefab, transform, true);
                powerUpObject.SetActive(false);
                pool.Enqueue(powerUpObject);
            }

            pools.Add(prefab, pool);
        }
    }

    public GameObject GetPowerUp(GameObject prefab)
    {
        if (prefab == null) return null;

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        GameObject powerUpObject = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, transform, true);
        if (powerUpObject == null) return null;

        activePowerUps[powerUpObject] = prefab;
        powerUpObject.SetActive(true);
        return powerUpObject;
    }

    public void ReturnPowerUp(GameObject powerUpObject)
    {
        if (powerUpObject == null) return;

        if (!activePowerUps.TryGetValue(powerUpObject, out GameObject prefab))
        {
            powerUpObject.transform.SetParent(transform);
            powerUpObject.SetActive(false);
            return;
        }

        if (!pools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            pools.Add(prefab, pool);
        }

        powerUpObject.transform.SetParent(transform);
        powerUpObject.SetActive(false);
        pool.Enqueue(powerUpObject);
        activePowerUps.Remove(powerUpObject);
    }

    public GameObject GetRandomPrefab(System.Random random)
    {
        if (random == null || powerUpsPrefabs == null || powerUpsPrefabs.Count == 0) return null;

        int prefabsCount = powerUpsPrefabs.Count;
        for (int i = 0; i < prefabsCount; i++)
        {
            int randomIndex = random.Next(0, prefabsCount);
            GameObject prefab = powerUpsPrefabs[randomIndex];
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }

    private void EnsureFallbackPrefab()
    {
        if (HasAnyPrefab()) return;

        PowerUpsTrigger fallbackTrigger = FindObjectOfType<PowerUpsTrigger>();
        if (fallbackTrigger == null) return;

        GameObject fallbackObject = fallbackTrigger.gameObject;
        powerUpsPrefabs.Add(fallbackObject);
        fallbackObject.SetActive(false);
    }

    private bool HasAnyPrefab()
    {
        if (powerUpsPrefabs == null || powerUpsPrefabs.Count == 0) return false;

        for (int i = 0; i < powerUpsPrefabs.Count; i++)
        {
            if (powerUpsPrefabs[i] != null)
            {
                return true;
            }
        }

        return false;
    }
}
