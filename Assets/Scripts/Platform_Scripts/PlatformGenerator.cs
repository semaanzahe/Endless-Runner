using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    public float platformZ = 20;
    public Transform cameraTransform;
    [Min(0)] public int platformsWithoutObstaclesAtStart = 5;

    private int platformCount;
    private float nextPosition;
    private int spawnedPlatformsCount;
    private readonly List<GameObject> activePlatforms = new List<GameObject>();

    private void Start()
    {
        if (PlatformPoolingSystem.instance == null || cameraTransform == null)
        {
            enabled = false;
            return;
        }

        platformCount = PlatformPoolingSystem.instance.numberOfPlatforms;
        nextPosition = 0f;
        spawnedPlatformsCount = 0;

        for (int i = 0; i < platformCount; i++)
        {
            SpawnPlatform();
        }
    }

    private void Update()
    {
        if (PlatformPoolingSystem.instance == null || cameraTransform == null) return;

        if (cameraTransform.position.z + (platformCount * platformZ) > nextPosition)
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
                EnvironmentGenerator environment = platform.GetComponent<EnvironmentGenerator>();
                if (environment != null)
                {
                    environment.UpdateEnvironment();
                }
                ObstaclesGenerator obstacles = platform.GetComponent<ObstaclesGenerator>();
                if (obstacles != null)
                {
                    obstacles.UpdateObstacles();
                }
                CoinsGenerator coin = platform.GetComponent<CoinsGenerator>();
                if (coin != null)
                {
                    coin.UpdateCoins();
                }

                activePlatforms.RemoveAt(i);
                PlatformPoolingSystem.instance.ReturnPlatform(platform);
            }
        }
    }

    private void SpawnPlatform()
    {
        GameObject platform = PlatformPoolingSystem.instance.GetPlatform();
        if (platform == null) return;

        platform.transform.position = new Vector3(0, platform.transform.position.y, nextPosition);
        nextPosition += platformZ;
        activePlatforms.Add(platform);
        bool hasObstacles = false;
        bool canSpawnObstacles = spawnedPlatformsCount >= platformsWithoutObstaclesAtStart;
        ObstaclesGenerator obstacles = platform.GetComponent<ObstaclesGenerator>();
        
        if (obstacles != null && canSpawnObstacles)
        {
            hasObstacles = obstacles.GenerateObstacles();
        }

        CoinsGenerator coin = platform.GetComponent<CoinsGenerator>();
        if (coin != null)
        {
            if (hasObstacles)
            {
                coin.UpdateCoins();
            }
            else
            {
                coin.GenerateCoins();
            }
        }

        EnvironmentGenerator environment = platform.GetComponent<EnvironmentGenerator>();
        if (environment != null)
        {
            environment.GenerateEnvironment();
        }

        spawnedPlatformsCount++;
    }
}
