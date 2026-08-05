using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns platform segments ahead of the camera and recycles them once passed.
/// Platforms are now fetched from the single ObjectPooler (by prefab tag) instead of
/// a dedicated PlatformPoolingSystem. Content generation on each platform (coins,
/// obstacles, environment) is delegated to that platform's PlatformPopulator.
/// </summary>
public class PlatformGenerator : MonoBehaviour
{
    [Header("Platform")]
    public GameObject platformPrefab;
    public float platformZ = 20;
    public Transform cameraTransform;
    public int numberOfPlatforms = 10;
    [Min(0)] public int platformsWithoutObstaclesAtStart = 5;

    private string platformTag;
    private float nextPosition;
    private int spawnedPlatformsCount;
    private readonly List<GameObject> activePlatforms = new List<GameObject>();

    private void Start()
    {
        if (ObjectPooler.instance == null || cameraTransform == null || platformPrefab == null)
        {
            enabled = false;
            return;
        }

        platformTag = platformPrefab.name;
        nextPosition = 0f;
        spawnedPlatformsCount = 0;

        for (int i = 0; i < numberOfPlatforms; i++)
        {
            SpawnPlatform();
        }
    }

    private void Update()
    {
        if (ObjectPooler.instance == null || cameraTransform == null) return;

        if (cameraTransform.position.z + (numberOfPlatforms * platformZ) > nextPosition)
        {
            SpawnPlatform();
        }

        CleanupPlatforms();
    }

    private void CleanupPlatforms()
    {
        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            GameObject platform = activePlatforms[i];

            if (platform.transform.position.z < cameraTransform.position.z)
            {
                PlatformPopulator populator = platform.GetComponent<PlatformPopulator>();
                if (populator != null)
                {
                    populator.ClearPlatform();
                }

                ReturnActivePowerUps(platform);

                activePlatforms.RemoveAt(i);
                ObjectPooler.instance.ReturnObject(platform, platformTag);
            }
        }
    }

    private void SpawnPlatform()
    {
        GameObject platform = ObjectPooler.instance.GetPooledObject(platformTag);
        if (platform == null) return;

        platform.transform.position = new Vector3(0, platform.transform.position.y, nextPosition);
        nextPosition += platformZ;
        activePlatforms.Add(platform);

        bool canSpawnObstacles = spawnedPlatformsCount >= platformsWithoutObstaclesAtStart;

        PlatformPopulator populator = platform.GetComponent<PlatformPopulator>();
        if (populator != null)
        {
            populator.PopulatePlatform(canSpawnObstacles);
        }

        spawnedPlatformsCount++;
    }

    private void ReturnActivePowerUps(GameObject platform)
    {
        if (platform == null) return;

        PowerUpsTrigger[] powerUps = platform.GetComponentsInChildren<PowerUpsTrigger>(true);
        for (int i = 0; i < powerUps.Length; i++)
        {
            PowerUpsTrigger powerUp = powerUps[i];
            if (powerUp == null || !powerUp.gameObject.activeSelf) continue;

            powerUp.ReturnToPool();
        }
    }
}