using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class EnvironmentGenerator : MonoBehaviour
{
    public List<Transform> farSpawnPoints = new List<Transform>();
    public List<Transform> middleSpawnPoints = new List<Transform>();
    [Range(0f, 1f)] public float spawnChance = 0.7f;

    private readonly Random random = new Random();
    private readonly List<GameObject> activeEnvironmentObjects = new List<GameObject>();
    private readonly List<GameObject> activeEnvironmentPrefabs = new List<GameObject>();
    private EnvironmentPoolingSystem environmentPool;

    public void GenerateEnvironment()
    {
        if (environmentPool == null)
        {
            environmentPool = GetComponent<EnvironmentPoolingSystem>();
        }

        if (environmentPool == null) return;

        activeEnvironmentObjects.Clear();
        activeEnvironmentPrefabs.Clear();

        SpawnCategory(farSpawnPoints, environmentPool.farObjectsPrefabs);
        SpawnCategory(middleSpawnPoints, environmentPool.middleObjectsPrefabs);
    }

    public void UpdateEnvironment()
    {
        if (environmentPool == null)
        {
            environmentPool = GetComponent<EnvironmentPoolingSystem>();
        }

        if (environmentPool == null) return;

        for (int i = 0; i < activeEnvironmentObjects.Count; i++)
        {
            environmentPool.ReturnEnvironmentObject(activeEnvironmentPrefabs[i], activeEnvironmentObjects[i]);
        }

        activeEnvironmentObjects.Clear();
        activeEnvironmentPrefabs.Clear();
    }

    private void SpawnCategory(List<Transform> spawnPoints, List<GameObject> prefabs)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        if (prefabs == null || prefabs.Count == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;
            if (random.NextDouble() > spawnChance) continue;

            GameObject prefab = GetRandomPrefab(prefabs);
            if (prefab == null) continue;

            GameObject environmentObject = environmentPool.GetEnvironmentObject(prefab);
            if (environmentObject == null) continue;

            environmentObject.transform.SetParent(spawnPoint);
            environmentObject.transform.position = spawnPoint.position;
            environmentObject.transform.rotation = spawnPoint.rotation;

            activeEnvironmentObjects.Add(environmentObject);
            activeEnvironmentPrefabs.Add(prefab);
        }
    }

    private GameObject GetRandomPrefab(List<GameObject> prefabs)
    {
        int prefabCount = prefabs.Count;
        if (prefabCount == 0) return null;

        for (int i = 0; i < prefabCount; i++)
        {
            int randomIndex = random.Next(0, prefabCount);
            GameObject prefab = prefabs[randomIndex];
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }
}
