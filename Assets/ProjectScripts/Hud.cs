using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Hud : MonoBehaviour
{
    
    public static Hud Instance;

    [Header("References")]
    [SerializeField] private Transform player;

    private Vector3 startPos;

    [Header("UI Text References")]
    public TextMeshProUGUI distance;
    public TextMeshProUGUI time;
    public TextMeshProUGUI Coin;
    public TextMeshProUGUI TotalCoin;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI HighestScore;

    [Header("Runtime Tracker Values")]
    private float elapstTime = 0f;
    private int coins;

    // Persistent data saved across sessions
    [Header("Saved Values")]
    public int totalCoins;
    public int highScore;

    [Header("Score Settings")]
    [HideInInspector] 
    public int scoreMultiplier = 1;

    private int score;

    // C# Expression-bodied property (automatically hidden from the Inspector)
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
         if (QuestManager.Instance != null)
         {
             QuestManager.Instance.AddProgress(MissionType.Score, score);
         }
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
        
        int finalScore = currentScore * scoreMultiplier;
        
        Score.text = $"{finalScore}";
        
        if (finalScore > highScore)
        {
            highScore = finalScore;
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
