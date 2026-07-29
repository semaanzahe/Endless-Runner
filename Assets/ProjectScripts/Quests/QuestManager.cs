using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestProgressData
{
    public string questID;
    public int currentProgress;
    public int currentStageIndex;
    public bool isCompleted;
}

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    // --- EVENTS ---
    public static event Action OnQuestProgressUpdated;
    public static event Action<string> OnQuestCompleted; // Passes questID

    [Header("Quest Database")]
    [SerializeField] private List<QuestSO> questDatabase = new List<QuestSO>();

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
                    questID = quest.questID,
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
        bool changed = false;

        foreach (var quest in questDatabase)
        {
            if (quest.questType != type) continue;
            if (!questDataDict.TryGetValue(quest.questID, out var data)) continue;
            if (data.isCompleted) continue;

            if (quest.executionScope == ExecutionScope.Cumulative)
            {
                data.currentProgress += amount;
                changed = true;
            }
            else if (quest.executionScope == ExecutionScope.InOneRun)
            {
                if (amount > data.currentProgress)
                {
                    data.currentProgress = amount;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            OnQuestProgressUpdated?.Invoke();
            // Automatically save progress when it updates
            SaveQuestData();
        }
    }

    // --- REWARD CLAIMING ---
    public void ClaimReward(string questID)
    {
        QuestSO quest = questDatabase.Find(q => q.questID == questID);
        if (quest == null || !questDataDict.TryGetValue(questID, out var data)) return;

        if (data.currentStageIndex < quest.stages.Count)
        {
            MissionStage currentStage = quest.stages[data.currentStageIndex];

            if (data.currentProgress >= currentStage.targetAmount)
            {
                GrantReward(currentStage.rewardType, currentStage.rewardAmount);
                data.currentStageIndex++;

                if (data.currentStageIndex >= quest.stages.Count)
                {
                    data.isCompleted = true;
                    OnQuestCompleted?.Invoke(questID);
                }

                OnQuestProgressUpdated?.Invoke();
                SaveQuestData();
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
                Debug.Log($"[QuestManager] Granted {amount} Keys!");
                break;
            case MissionRewardType.ScoreMultiplier:
                if (Hud.Instance != null) Hud.Instance.scoreMultiplier = amount;
                Debug.Log($"[QuestManager] Granted +{amount} Score Multiplier!");
                break;
        }
    }

    // --- SAVE & LOAD INTEGRATION ---

    /// <summary>
    /// Call this to get a list of all current quest states for saving.
    /// </summary>
    public List<QuestProgressData> GetSaveData()
    {
        return new List<QuestProgressData>(questDataDict.Values);
    }

    /// <summary>
    /// Call this when loading saved data from file/JSON.
    /// </summary>
    public void LoadSaveData(List<QuestProgressData> savedDataList)
    {
        if (savedDataList == null) return;

        foreach (var savedData in savedDataList)
        {
            if (questDataDict.ContainsKey(savedData.questID))
            {
                questDataDict[savedData.questID] = savedData;
            }
        }

        OnQuestProgressUpdated?.Invoke();
    }

    private void SaveQuestData()
    {
        if (Serializator.instance != null)
        {
            Serializator.instance.SaveDataWithoutScreenshot();
        }
    }
}