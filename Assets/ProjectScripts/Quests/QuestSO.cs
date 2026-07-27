using System;
using System.Collections.Generic;
using UnityEngine;

public enum MissionType
{
    Score,               // Reach X score
    CoinsCollected,      // Collect X coins
    DistanceTraveled,    // Run X distance
}

public enum ExecutionScope
{
    InOneRun,            // Must be done in a single run
    Cumulative           // Across multiple runs / lifetime
}

public enum MissionConstraint
{
    None,
    NoCoinCollection,    // Can't pick up coins
    ExactCoins           // Must end run with exact coin count
}

public enum MissionRewardType
{
    Keys,
    Coins,
    ScoreMultiplier
}

[System.Serializable]
public class MissionStage
{
    public int targetAmount;       // Goal for this stage (e.g. 15,000 -> 30,000 -> 60,000)
    public int rewardAmount = 3;   // Reward for completing this stage
    public MissionRewardType rewardType = MissionRewardType.Keys;
}

[CreateAssetMenu(fileName = "Achievement_", menuName = "Missions/AchievementSO")]
public class QuestSO : ScriptableObject
{
    [Header("Achievement Identity")]
    public string achievementID; // Unique ID for JSON save (e.g. "ach_no_acrobatics")
    public string title;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("Rules & Constraints")]
    public MissionType missionType;
    public ExecutionScope executionScope = ExecutionScope.InOneRun;
    public MissionConstraint constraint = MissionConstraint.None;

    [Header("Stages (e.g., 4 Spray Cans)")]
    [Tooltip("Define the targets for Stage 1, Stage 2, Stage 3, Stage 4")]
    public List<MissionStage> stages = new List<MissionStage>();
}