using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ObstaclesGenerator : MonoBehaviour
{
    public List<Transform> lines = new List<Transform>();
    [Range(0f, 1f)] public float platformObstacleChance = 0.45f;
    [Range(0f, 1f)] public float laneObstacleChance = 0.7f;
    [Min(0)] public int maxObstaclesPerPlatform = 3;

    private readonly Random random = new Random();
    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly List<GameObject> activeObstaclePrefabs = new List<GameObject>();
    private readonly List<GameObject> runtimeAllowedObstacleTypes = new List<GameObject>();
    private ObstaclesPoolingSystem obstaclesPool;

    public void ApplyDifficulty(DifficultySystem difficulty)
    {
        if (difficulty == null) return;

        platformObstacleChance = Mathf.Clamp01(difficulty.spawnRate);
        laneObstacleChance = Mathf.Clamp01(difficulty.spawnRate);
        maxObstaclesPerPlatform = Mathf.Max(0, difficulty.maxObstacles);

        runtimeAllowedObstacleTypes.Clear();
        if (difficulty.allowedObstacleTypes == null) return;

        for (int i = 0; i < difficulty.allowedObstacleTypes.Count; i++)
        {
            GameObject obstacleType = difficulty.allowedObstacleTypes[i];
            if (obstacleType == null) continue;

            runtimeAllowedObstacleTypes.Add(obstacleType);
        }
    }

    public bool GenerateObstacles()
    {
        if (obstaclesPool == null)
        {
            obstaclesPool = GetComponent<ObstaclesPoolingSystem>();
        }

        if (obstaclesPool == null || lines == null || lines.Count == 0) return false;
        if (maxObstaclesPerPlatform <= 0) return false;
        if (!HasAnyAvailableObstaclePrefab()) return false;

        activeObstacles.Clear();
        activeObstaclePrefabs.Clear();

        if (random.NextDouble() > platformObstacleChance) return false;

        int safeLaneIndex = random.Next(0, lines.Count);
        bool spawnedAny = false;
        int spawnedCount = 0;
        int positionsCount = lines[0].childCount;

        for (int laneIndex = 0; laneIndex < lines.Count && spawnedCount < maxObstaclesPerPlatform; laneIndex++)
        {
            if (laneIndex == safeLaneIndex) continue;
            Transform lane = lines[laneIndex];
            if (lane == null) continue;

            for (int i = 0; i < positionsCount && spawnedCount < maxObstaclesPerPlatform; i++)
            {
                if (i >= lane.childCount) continue;
                if (random.NextDouble() > laneObstacleChance) continue;

                Transform spawnPoint = lane.GetChild(i);
                GameObject prefab = GetRandomObstaclePrefab();
                if (prefab == null) continue;

                SpawnObstacle(prefab, spawnPoint);
                spawnedAny = true;
                spawnedCount++;
            }
        }

        if (!spawnedAny && spawnedCount < maxObstaclesPerPlatform)
        {
            spawnedAny = SpawnFallbackObstacle(safeLaneIndex);
        }

        return spawnedAny;
    }

    public void UpdateObstacles()
    {
        if (obstaclesPool == null)
        {
            obstaclesPool = GetComponent<ObstaclesPoolingSystem>();
        }

        if (obstaclesPool == null) return;

        for (int i = 0; i < activeObstacles.Count; i++)
        {
            obstaclesPool.ReturnObstacle(activeObstaclePrefabs[i], activeObstacles[i]);
        }

        activeObstacles.Clear();
        activeObstaclePrefabs.Clear();
    }

    private bool SpawnFallbackObstacle(int safeLaneIndex)
    {
        List<int> possibleLanes = new List<int>();

        for (int laneIndex = 0; laneIndex < lines.Count; laneIndex++)
        {
            if (laneIndex == safeLaneIndex) continue;
            if (lines[laneIndex] == null || lines[laneIndex].childCount == 0) continue;

            possibleLanes.Add(laneIndex);
        }

        if (possibleLanes.Count == 0) return false;

        int randomLane = possibleLanes[random.Next(0, possibleLanes.Count)];
        Transform lane = lines[randomLane];
        int randomSpot = random.Next(0, lane.childCount);

        GameObject prefab = GetRandomObstaclePrefab();
        if (prefab == null) return false;

        SpawnObstacle(prefab, lane.GetChild(randomSpot));
        return true;
    }

    private void SpawnObstacle(GameObject prefab, Transform spawnPoint)
    {
        GameObject obstacleObject = obstaclesPool.GetObstacle(prefab);
        if (obstacleObject == null) return;

        obstacleObject.transform.SetParent(spawnPoint);
        obstacleObject.transform.position = spawnPoint.position;
        obstacleObject.transform.rotation = spawnPoint.rotation;

        activeObstacles.Add(obstacleObject);
        activeObstaclePrefabs.Add(prefab);
    }

    private GameObject GetRandomObstaclePrefab()
    {
        if (runtimeAllowedObstacleTypes.Count > 0)
        {
            int allowedCount = runtimeAllowedObstacleTypes.Count;
            for (int i = 0; i < allowedCount; i++)
            {
                int randomIndex = random.Next(0, allowedCount);
                GameObject prefab = runtimeAllowedObstacleTypes[randomIndex];
                if (prefab != null)
                {
                    return prefab;
                }
            }
        }

        int prefabCount = obstaclesPool.obstaclesPrefabs.Count;
        if (prefabCount == 0) return null;

        for (int i = 0; i < prefabCount; i++)
        {
            int randomIndex = random.Next(0, prefabCount);
            GameObject prefab = obstaclesPool.obstaclesPrefabs[randomIndex];
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }

    private bool HasAnyAvailableObstaclePrefab()
    {
        if (runtimeAllowedObstacleTypes.Count > 0)
        {
            for (int i = 0; i < runtimeAllowedObstacleTypes.Count; i++)
            {
                if (runtimeAllowedObstacleTypes[i] != null)
                {
                    return true;
                }
            }
        }

        if (obstaclesPool.obstaclesPrefabs == null || obstaclesPool.obstaclesPrefabs.Count == 0) return false;

        for (int i = 0; i < obstaclesPool.obstaclesPrefabs.Count; i++)
        {
            if (obstaclesPool.obstaclesPrefabs[i] != null)
            {
                return true;
            }
        }

        return false;
    }
}
