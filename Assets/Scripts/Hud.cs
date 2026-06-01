using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class Hud : MonoBehaviour
{
    
    public Transform player;
    
    private Vector3 startPos;
    
     public TextMeshProUGUI distance;
     
    public TextMeshProUGUI time;
    private float elapstTime=0f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = player.position;
    }

    // Update is called once per frame
    void Update()
    {
        elapstTime += Time.deltaTime;
        updateTime(elapstTime);
        updateDistance();
    }


    private void updateDistance()
    {
        float dist= Vector3.Distance(player.position, startPos);
        Math.Abs(dist);
        distance.text = "Distance: \n" + dist.ToString("F0");
    }

    private void updateTime(float displaytime)
    {
        int minutes = Mathf.FloorToInt(displaytime / 60);
        int seconds = Mathf.FloorToInt(displaytime % 60);
        int miliseconds = Mathf.FloorToInt((displaytime % 1) * 100);
        
        
        time.text = "Time: \n"+ string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, miliseconds);
    }
}
