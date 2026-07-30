using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(PowerUpsPoolingSystem))]
[DisallowMultipleComponent]

public class PowerUpsGenerator : MonoBehaviour
{

    private readonly Random random = new Random();
    private PowerUpsPoolingSystem powerUpsPool;

    private void Awake()
    {
        powerUpsPool = GetComponent<PowerUpsPoolingSystem>();
    }

    public bool SpawnPowerUpOnSpawnPoint(List<GameObject> allowedPowerUps, Transform spawnPoint)
{
    if (allowedPowerUps == null || allowedPowerUps.Count == 0) return false;
    if (spawnPoint == null) return false;

    if (powerUpsPool == null)
    {
        powerUpsPool = GetComponent<PowerUpsPoolingSystem>();
    }

    if (powerUpsPool == null) return false;

    // 1. Pick a random PowerUp Prefab from the allowed list
    GameObject chosenPrefab = GetRandomPowerUpPrefab(allowedPowerUps);
    if (chosenPrefab == null) return false;

    // 2. Fetch an instance of that specific prefab from your pooling system
    GameObject powerUpObject = powerUpsPool.GetPowerUp(chosenPrefab);
    if (powerUpObject == null) return false;

    // 3. Position the object at the spawn point
    powerUpObject.transform.SetParent(spawnPoint);
    powerUpObject.transform.position = spawnPoint.position;
    powerUpObject.transform.rotation = spawnPoint.rotation;

    // 4. Ensure trigger script and collider settings exist on the pooled instance
    PowerUpsTrigger trigger = powerUpObject.GetComponent<PowerUpsTrigger>();
    if (trigger == null)
    {
        trigger = powerUpObject.AddComponent<PowerUpsTrigger>();
    }

    Collider powerUpCollider = powerUpObject.GetComponent<Collider>();
    if (powerUpCollider == null)
    {
        BoxCollider boxCollider = powerUpObject.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
    }
    else
    {
        powerUpCollider.isTrigger = true;
    }

    // Link the pooling reference so it knows where to return when collected
    trigger.SetPool(powerUpsPool);

    return true;
}

private GameObject GetRandomPowerUpPrefab(List<GameObject> allowedPowerUps)
{
    int count = allowedPowerUps.Count;

    for (int i = 0; i < count; i++)
    {
        int randomIndex = random.Next(0, count);
        GameObject prefab = allowedPowerUps[randomIndex];
        if (prefab != null)
        {
            return prefab;
        }
    }

    return null;
}
}
