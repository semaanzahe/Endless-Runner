using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestProgressData
{
    public string achievementID;
    public int currentProgress;
    public int currentStageIndex;
    public bool isCompleted;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Quest Database")]
    [SerializeField] private List<QuestSO> questDatabase = new List<QuestSO>();

    // Dictionary for fast lookup at runtime: ID -> Progress
    private Dictionary<string, QuestProgressData> questDataDict = new Dictionary<string, QuestProgressData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeQuests();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeQuests()
    {
        questDataDict.Clear();
        foreach (var quest in questDatabase)
        {
            if (quest != null && !questDataDict.ContainsKey(quest.questID))
            {
                questDataDict.Add(quest.questID, new QuestProgressData
                {
                    achievementID = quest.questID,
                    currentProgress = 0,
                    currentStageIndex = 0,
                    isCompleted = false
                });
            }
        }
    }

    public List<QuestSO> GetAllQuests() => questDatabase;

    public int GetQuestProgress(string id) => questDataDict.TryGetValue(id, out var data) ? data.currentProgress : 0;
    public int GetQuestStage(string id) => questDataDict.TryGetValue(id, out var data) ? data.currentStageIndex : 0;
    public bool IsQuestCompleted(string id) => questDataDict.TryGetValue(id, out var data) && data.isCompleted;

    // --- PROGRESS TRACKING ---
    public void AddProgress(MissionType type, int amount)
    {
        foreach (var quest in questDatabase)
        {
            if (quest.questType != type) continue;
            if (!questDataDict.TryGetValue(quest.questID, out var data)) continue;
            if (data.isCompleted) continue;

            if (quest.executionScope == ExecutionScope.Cumulative)
            {
                data.currentProgress += amount;
            }
            else if (quest.executionScope == ExecutionScope.InOneRun)
            {
                data.currentProgress = Mathf.Max(data.currentProgress, amount);
            }
        }

        QuestUIManager.Instance?.RefreshUI();
    }

    // --- REWARD CLAIMING ---
    public void ClaimReward(string achievementID)
    {
        QuestSO quest = questDatabase.Find(q => q.questID == achievementID);
        if (quest == null || !questDataDict.TryGetValue(achievementID, out var data)) return;

        if (data.currentStageIndex < quest.stages.Count)
        {
            MissionStage currentStage = quest.stages[data.currentStageIndex];

            // Verify they hit the target
            if (data.currentProgress >= currentStage.targetAmount)
            {
                // Grant Reward based on type
                GrantReward(currentStage.rewardType, currentStage.rewardAmount);

                // Advance to next stage
                data.currentStageIndex++;

                // Check if all stages are done
                if (data.currentStageIndex >= quest.stages.Count)
                {
                    data.isCompleted = true;
                }

                // Update UI and trigger game save
                QuestUIManager.Instance?.RefreshUI();
                Serializator.instance?.SaveDataWithoutScreenshot();
            }
        }
    }

    private void GrantReward(MissionRewardType type, int amount)
    {
        switch (type)
        {
            case MissionRewardType.Coins:
                if (Hud.Instance != null) Hud.Instance.totalCoins += amount;
                break;
            case MissionRewardType.Keys:
                // Grant keys logic here
                Debug.Log($"[QuestManager] Granted {amount} Keys!");
                break;
            case MissionRewardType.ScoreMultiplier:
                // Add multiplier logic here
                Debug.Log($"[QuestManager] Granted +{amount} Score Multiplier!");
                break;
        }
    }

    // --- SAVE / LOAD DATA ---
    public List<QuestProgressData> GetSaveData()
    {
        return new List<QuestProgressData>(questDataDict.Values);
    }

    public void LoadSaveData(List<QuestProgressData> savedList)
    {
        if (savedList == null) return;

        foreach (var savedData in savedList)
        {
            if (questDataDict.ContainsKey(savedData.achievementID))
            {
                questDataDict[savedData.achievementID] = savedData;
            }
        }
    }
}