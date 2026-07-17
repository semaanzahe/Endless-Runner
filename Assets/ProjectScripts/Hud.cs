using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Hud : MonoBehaviour
{
    
    public static Hud Instance;
    public Transform player;
    
    private Vector3 startPos;
    
     public TextMeshProUGUI distance;
     
    public TextMeshProUGUI time;
    private float elapstTime=0f;
    
    public TextMeshProUGUI Coin;
    public TextMeshProUGUI TotalCoin;
    
    private int coins;
    public int totalCoins;

    public TextMeshProUGUI Score;
    public TextMeshProUGUI HighestScore;
    
    private int score;
    public int highScore;
    public int CurrentScore => score;

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        startPos = player.position;
        
        // Call these AFTER Serializator has finished loading data in its Awake method
        UpdateCoins();
        UpdateTotalCoinsUI();
        UpdateHighScoreUI();
    }

    // Update is called once per frame
    void Update()
    {
        
        elapstTime += Time.deltaTime;
        float dist= Vector3.Distance(player.position, startPos);
        updateTime(elapstTime);
        updateDistance(dist);
        UpdateCoins();
         score = (int)(coins*10+elapstTime+(int)dist);
        UpdateScore(score);
    }


    private void updateDistance(float dist)
    {
        distance.text =dist.ToString("F0");
    }

    private void updateTime(float displaytime)
    {
        int minutes = Mathf.FloorToInt(displaytime / 60);
        int seconds = Mathf.FloorToInt(displaytime % 60);
        int miliseconds = Mathf.FloorToInt((displaytime % 1) * 100);
        
        
        time.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, miliseconds);
    }

    public void AddCoin()
    {
        coins += 1;      
        totalCoins += 1; 
        
        UpdateCoins();
        UpdateTotalCoinsUI();
    }

    private void UpdateCoins()
    {
        Coin.text = $"{coins}";
    }
    
    private void UpdateTotalCoinsUI()
    {
        if (TotalCoin != null)
        {
            TotalCoin.text = $"{totalCoins}";
        }
    }

    private void UpdateScore(int currentScore)
    {
        if (Score == null) return;

        Score.text = $"{currentScore}";

        
        if (currentScore > highScore)
        {
            highScore = currentScore;
            
            UpdateHighScoreUI();
        }
        
    }
    private void UpdateHighScoreUI()
    {
        if (HighestScore != null)
        {
            HighestScore.text = $"{highScore}";
        }
    }
    
}
