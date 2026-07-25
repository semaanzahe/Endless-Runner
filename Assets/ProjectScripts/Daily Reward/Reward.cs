using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Reward : MonoBehaviour
{
    public DailyRewardSO reward;
    
    [SerializeField]
    private Image icon;
    [SerializeField]
    private TMP_Text rewardAmount;

    private void Start()
    {
        icon.sprite = reward.icon;
        rewardAmount.SetText(reward.rewardAmount.ToString());

    }
}
