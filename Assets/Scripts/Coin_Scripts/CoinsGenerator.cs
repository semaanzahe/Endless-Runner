using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class CoinsGenerator : MonoBehaviour
{
    public List<Transform> lines;

    private readonly Random random = new Random();
    private readonly List<GameObject> activeCoins = new List<GameObject>();
    private CoinsPoolingSystem coinsPool;

    public void GenerateCoins()
    {
        if (coinsPool == null)
        {
            coinsPool = GetComponent<CoinsPoolingSystem>();
        }

        if (coinsPool == null || lines == null || lines.Count == 0) return;

        activeCoins.Clear();
        int positionsCount = lines[0].childCount;

        for (int i = 0; i < positionsCount; i++)
        {
            int randomLane = random.Next(0, lines.Count);
            if (i >= lines[randomLane].childCount) continue;

            Transform child = lines[randomLane].GetChild(i);
            GameObject coin = coinsPool.GetCoin();
            if (coin == null) continue;

            activeCoins.Add(coin);
            coin.transform.SetParent(child);
            coin.transform.position = child.position;
            coin.transform.rotation = child.rotation;
        }
    }

    public void UpdateCoins()
    {
        if (coinsPool == null)
        {
            coinsPool = GetComponent<CoinsPoolingSystem>();
        }

        if (coinsPool == null) return;

        foreach (GameObject coin in activeCoins)
        {
            coinsPool.ReturnCoin(coin);
        }

        activeCoins.Clear();
    }
}
