using System.Collections.Generic;
using UnityEngine;

public class PlatformPoolingSystem : MonoBehaviour
{
    public static PlatformPoolingSystem instance;

    public GameObject platformPrefab;
    public int numberOfPlatforms;

    private readonly Queue<GameObject> Platforms = new Queue<GameObject>();

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        for (int i = 0; i < numberOfPlatforms; i++)
        {
            GameObject platform = Instantiate(platformPrefab, transform, true);
            platform.SetActive(false);
            Platforms.Enqueue(platform);
        }
    }

    public GameObject GetPlatform()
    {
        if (Platforms.Count > 0)
        {
            GameObject platform = Platforms.Dequeue();
            platform.SetActive(true);
            return platform;
        }

        GameObject newPlatform = Instantiate(platformPrefab, transform, true);
        newPlatform.SetActive(true);
        return newPlatform;
    }

    public void ReturnPlatform(GameObject platform)
    {
        if (platform == null) return;

        platform.transform.SetParent(transform);
        platform.SetActive(false);
        Platforms.Enqueue(platform);
    }
}
