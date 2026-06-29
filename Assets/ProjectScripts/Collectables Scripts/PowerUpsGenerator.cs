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

    public bool SpawnPowerUpOnSpawnPoint(List<PowerUp> allowedPowerUps, Transform spawnPoint)
    {
        if (allowedPowerUps == null || allowedPowerUps.Count == 0) return false;
        if (spawnPoint == null) return false;

        if (powerUpsPool == null)
        {
            powerUpsPool = GetComponent<PowerUpsPoolingSystem>();
        }

        if (powerUpsPool == null) return false;

        GameObject prefab = powerUpsPool.GetRandomPrefab(random);
        if (prefab == null) return false;

        PowerUp powerUpData = GetRandomPowerUp(allowedPowerUps);
        if (powerUpData == null) return false;

        GameObject powerUpObject = powerUpsPool.GetPowerUp(prefab);
        if (powerUpObject == null) return false;
        powerUpObject.transform.SetParent(spawnPoint);
        powerUpObject.transform.position = spawnPoint.position;
        powerUpObject.transform.rotation = spawnPoint.rotation;

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

        trigger.powerUp = powerUpData;
        trigger.SetPool(powerUpsPool);

        return true;
    }

    private PowerUp GetRandomPowerUp(List<PowerUp> allowedPowerUps)
    {
        int count = allowedPowerUps.Count;

        for (int i = 0; i < count; i++)
        {
            int randomIndex = random.Next(0, count);
            PowerUp powerUp = allowedPowerUps[randomIndex];
            if (powerUp != null)
            {
                return powerUp;
            }
        }

        return null;
    }
}
