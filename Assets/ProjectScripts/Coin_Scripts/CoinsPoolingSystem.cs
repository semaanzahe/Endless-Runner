using System.Collections.Generic;
using UnityEngine;

public class CoinsPoolingSystem : MonoBehaviour
{
    public GameObject CoinPrefab;

    public int linesCount;
    public int spawnAreasAmount;
    public int platformAmount;

    private int CoinsAmount;
    private readonly Queue<GameObject> Coins = new Queue<GameObject>();

    private void Awake()
    {
        CoinsAmount = linesCount * spawnAreasAmount * platformAmount;
        if (CoinsAmount < 1)
        {
            CoinsAmount = 1;
        }

        for (int i = 0; i < CoinsAmount; i++)
        {
            GameObject coin = Instantiate(CoinPrefab, transform, true);
            coin.SetActive(false);
            Coins.Enqueue(coin);
        }
    }

    public GameObject GetCoin()
    {
        if (Coins.Count > 0)
        {
            GameObject coin = Coins.Dequeue();
            coin.SetActive(true);
            return coin;
        }

        GameObject newCoin = Instantiate(CoinPrefab, transform, true);
        newCoin.SetActive(true);
        return newCoin;
    }

    public void ReturnCoin(GameObject coin)
    {
        if (coin == null) return;

        coin.transform.SetParent(transform);
        coin.SetActive(false);
        Coins.Enqueue(coin);
    }
}
