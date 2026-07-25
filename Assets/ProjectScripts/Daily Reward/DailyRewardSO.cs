using UnityEngine;

public enum RewardType
{
    Gold
}
[CreateAssetMenu(fileName = "NewDailyReward", menuName = "Daily Reward/Reward")]
public class DailyRewardSO : ScriptableObject
{
    public int rewardAmount;
    public RewardType rewardType;
    public Sprite icon;
}
