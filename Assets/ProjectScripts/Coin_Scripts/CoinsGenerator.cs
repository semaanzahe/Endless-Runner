using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class CoinsGenerator : MonoBehaviour
{
    public List<Transform> lines;

    private readonly Random random = new Random();
    private readonly List<GameObject> activeCoins = new List<GameObject>();
    private CoinsPoolingSystem coinsPool;
    private int[] laneUsedByPosition = new int[0];

    public void GenerateCoins()
    {
        if (coinsPool == null)
        {
            coinsPool = GetComponent<CoinsPoolingSystem>();
        }

        if (coinsPool == null || lines == null || lines.Count == 0) return;

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
            GameObject coin = coinsPool.GetCoin();
            if (coin == null) continue;

            activeCoins.Add(coin);
            laneUsedByPosition[i] = randomLane;
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
        laneUsedByPosition = new int[0];
    }

    public bool HasCoinsOnPlatform()
    {
        return activeCoins.Count > 0;
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
}
