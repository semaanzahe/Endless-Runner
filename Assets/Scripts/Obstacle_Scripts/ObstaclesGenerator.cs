using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ObstaclesGenerator : MonoBehaviour
{
    public List<Transform> lines = new List<Transform>();
    [Range(0f, 1f)] public float platformObstacleChance = 0.45f;
    [Range(0f, 1f)] public float laneObstacleChance = 0.7f;

    private readonly Random random = new Random();
    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly List<GameObject> activeObstaclePrefabs = new List<GameObject>();
    private ObstaclesPoolingSystem obstaclesPool;

    public bool GenerateObstacles()
    {
        if (obstaclesPool == null)
        {
            obstaclesPool = GetComponent<ObstaclesPoolingSystem>();
        }

        if (obstaclesPool == null || lines == null || lines.Count == 0) return false;
        if (obstaclesPool.obstaclesPrefabs == null || obstaclesPool.obstaclesPrefabs.Count == 0) return false;

        activeObstacles.Clear();
        activeObstaclePrefabs.Clear();

        if (random.NextDouble() > platformObstacleChance) return false;

        int safeLaneIndex = random.Next(0, lines.Count);
        bool spawnedAny = false;
        int positionsCount = lines[0].childCount;

        for (int laneIndex = 0; laneIndex < lines.Count; laneIndex++)
        {
            if (laneIndex == safeLaneIndex) continue;
            Transform lane = lines[laneIndex];
            if (lane == null) continue;

            for (int i = 0; i < positionsCount; i++)
            {
                if (i >= lane.childCount) continue;
                if (random.NextDouble() > laneObstacleChance) continue;

                Transform spawnPoint = lane.GetChild(i);
                GameObject prefab = GetRandomObstaclePrefab();
                if (prefab == null) continue;

                SpawnObstacle(prefab, spawnPoint);
                spawnedAny = true;
            }
        }

        if (!spawnedAny)
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
}
