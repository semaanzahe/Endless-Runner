using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

/// <summary>
/// Lives on the platform prefab. Merges what used to be CoinsGenerator, ObstaclesGenerator
/// and EnvironmentGenerator into one script, all pulling/returning objects through the
/// shared ObjectPooler instead of three separate dedicated pools.
/// </summary>
public class PlatformPopulator : MonoBehaviour
{
    [Header("Lanes (shared by coins & obstacles)")]
    public List<Transform> lines;

    [Header("Coins")]
    public GameObject coinPrefab;

    [Header("Obstacles")]
    public List<GameObject> obstaclesPrefabs = new List<GameObject>();
    [Range(0f, 1f)] public float platformObstacleChance = 0.45f;
    [Range(0f, 1f)] public float laneObstacleChance = 0.7f;
    [Min(0)] public int maxObstaclesPerPlatform = 3;

    [Header("Environment")]
    public List<Transform> farSpawnPoints = new List<Transform>();
    public List<Transform> middleSpawnPoints = new List<Transform>();
    public List<GameObject> farObjectsPrefabs = new List<GameObject>();
    public List<GameObject> middleObjectsPrefabs = new List<GameObject>();
    [Range(0f, 1f)] public float environmentSpawnChance = 0.7f;

    private readonly Random random = new Random();

    // Coins tracking
    private readonly List<GameObject> activeCoins = new List<GameObject>();
    private int[] laneUsedByPosition = new int[0];

    // Obstacles tracking
    private readonly List<GameObject> activeObstacles = new List<GameObject>();
    private readonly List<GameObject> activeObstaclePrefabs = new List<GameObject>();
    private readonly List<GameObject> runtimeAllowedObstacleTypes = new List<GameObject>();

    // Environment tracking
    private readonly List<GameObject> activeEnvironmentObjects = new List<GameObject>();
    private readonly List<GameObject> activeEnvironmentPrefabs = new List<GameObject>();

    public bool HasCoinsOnPlatform => activeCoins.Count > 0;

    // ---------------------------------------------------------------
    // Entry points called by PlatformGenerator
    // ---------------------------------------------------------------

    public void PopulatePlatform(bool canSpawnObstacles)
    {
        bool hasObstacles = canSpawnObstacles && GenerateObstacles();

        if (hasObstacles)
        {
            ClearCoins();
        }
        else
        {
            GenerateCoins();

            if (DifficultyManager.instance != null)
            {
                // NOTE: DifficultyManager.TrySpawnQueuedPowerUpOnCoinPlatform previously took a
                // CoinsGenerator. It now needs to accept this PlatformPopulator instead
                // (it can call TryGetEmptyLaneSpawnPoint on it the same way).
                DifficultyManager.instance.TrySpawnQueuedPowerUpOnCoinPlatform(this);
            }
        }

        GenerateEnvironment();
    }

    public void ClearPlatform()
    {
        ClearEnvironment();
        ClearObstacles();
        ClearCoins();
    }

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

    public bool TryGetEmptyLaneSpawnPoint(out Transform spawnPoint)
    {
        spawnPoint = null;
        if (lines == null || lines.Count == 0) return false;

        bool[] laneHasCoins = new bool[lines.Count];
        for (int i = 0; i < laneUsedByPosition.Length; i++)
        {
            int laneIndex = laneUsedByPosition[i];
            if (laneIndex >= 0 && laneIndex < laneHasCoins.Length)
            {
                laneHasCoins[laneIndex] = true;
            }
        }

        List<int> emptyLanes = new List<int>();
        for (int i = 0; i < lines.Count; i++)
        {
            if (laneHasCoins[i]) continue;
            if (lines[i] == null || lines[i].childCount == 0) continue;

            emptyLanes.Add(i);
        }

        if (emptyLanes.Count == 0) return false;

        int randomEmptyLane = emptyLanes[random.Next(0, emptyLanes.Count)];
        Transform emptyLane = lines[randomEmptyLane];
        int spawnIndex = emptyLane.childCount > 1 ? 1 : 0;
        spawnPoint = emptyLane.GetChild(spawnIndex);
        return spawnPoint != null;
    }

    // ---------------------------------------------------------------
    // Coins
    // ---------------------------------------------------------------

    private void GenerateCoins()
    {
        if (ObjectPooler.instance == null || coinPrefab == null || lines == null || lines.Count == 0) return;

        activeCoins.Clear();
        int positionsCount = lines[0].childCount;
        laneUsedByPosition = new int[positionsCount];
        for (int i = 0; i < laneUsedByPosition.Length; i++)
        {
            laneUsedByPosition[i] = -1;
        }

        for (int i = 0; i < positionsCount; i++)
        {
            int randomLane = random.Next(0, lines.Count);
            if (i >= lines[randomLane].childCount) continue;

            Transform child = lines[randomLane].GetChild(i);
            GameObject coin = ObjectPooler.instance.GetPooledObject(coinPrefab);
            if (coin == null) continue;

            activeCoins.Add(coin);
            laneUsedByPosition[i] = randomLane;
            coin.transform.SetParent(child);
            coin.transform.position = child.position;
            coin.transform.rotation = child.rotation;
        }
    }

    private void ClearCoins()
    {
        if (ObjectPooler.instance == null) return;

        for (int i = 0; i < activeCoins.Count; i++)
        {
            ObjectPooler.instance.ReturnObject(activeCoins[i], coinPrefab);
        }

        activeCoins.Clear();
        laneUsedByPosition = new int[0];
    }

    // ---------------------------------------------------------------
    // Obstacles
    // ---------------------------------------------------------------

    private bool GenerateObstacles()
    {
        if (ObjectPooler.instance == null || lines == null || lines.Count == 0) return false;
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

    private void ClearObstacles()
    {
        if (ObjectPooler.instance == null) return;

        for (int i = 0; i < activeObstacles.Count; i++)
        {
            ObjectPooler.instance.ReturnObject(activeObstacles[i], activeObstaclePrefabs[i]);
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
        GameObject obstacleObject = ObjectPooler.instance.GetPooledObject(prefab);
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
                if (prefab != null) return prefab;
            }
        }

        int prefabCount = obstaclesPrefabs.Count;
        if (prefabCount == 0) return null;

        for (int i = 0; i < prefabCount; i++)
        {
            int randomIndex = random.Next(0, prefabCount);
            GameObject prefab = obstaclesPrefabs[randomIndex];
            if (prefab != null) return prefab;
        }

        return null;
    }

    private bool HasAnyAvailableObstaclePrefab()
    {
        if (runtimeAllowedObstacleTypes.Count > 0)
        {
            for (int i = 0; i < runtimeAllowedObstacleTypes.Count; i++)
            {
                if (runtimeAllowedObstacleTypes[i] != null) return true;
            }
        }

        if (obstaclesPrefabs == null || obstaclesPrefabs.Count == 0) return false;

        for (int i = 0; i < obstaclesPrefabs.Count; i++)
        {
            if (obstaclesPrefabs[i] != null) return true;
        }

        return false;
    }

    // ---------------------------------------------------------------
    // Environment
    // ---------------------------------------------------------------

    private void GenerateEnvironment()
    {
        if (ObjectPooler.instance == null) return;

        activeEnvironmentObjects.Clear();
        activeEnvironmentPrefabs.Clear();

        SpawnEnvironmentCategory(farSpawnPoints, farObjectsPrefabs);
        SpawnEnvironmentCategory(middleSpawnPoints, middleObjectsPrefabs);
    }

    private void ClearEnvironment()
    {
        if (ObjectPooler.instance == null) return;

        for (int i = 0; i < activeEnvironmentObjects.Count; i++)
        {
            ObjectPooler.instance.ReturnObject(activeEnvironmentObjects[i], activeEnvironmentPrefabs[i]);
        }

        activeEnvironmentObjects.Clear();
        activeEnvironmentPrefabs.Clear();
    }

    private void SpawnEnvironmentCategory(List<Transform> spawnPoints, List<GameObject> prefabs)
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return;
        if (prefabs == null || prefabs.Count == 0) return;

        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null) continue;
            if (random.NextDouble() > environmentSpawnChance) continue;

            GameObject prefab = GetRandomPrefab(prefabs);
            if (prefab == null) continue;

            GameObject environmentObject = ObjectPooler.instance.GetPooledObject(prefab);
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
            if (prefab != null) return prefab;
        }

        return null;
    }
}