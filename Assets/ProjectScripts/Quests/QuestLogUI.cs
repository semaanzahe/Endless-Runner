using System.Collections.Generic;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager Instance;

    [Header("UI References")]
    [SerializeField] private Transform questContainer; // Parent Transform (e.g. Content inside ScrollView)
    [SerializeField] private GameObject questItemPrefab; // Prefab with QuestUIItem attached

    private List<QuestUIItem> spawnedItems = new List<QuestUIItem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (QuestManager.Instance == null) return;

        // Clear existing spawned slots
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();

        // Populate slots for all registered Quests
        List<QuestSO> allQuests = QuestManager.Instance.GetAllQuests();

        foreach (QuestSO quest in allQuests)
        {
            if (quest == null) continue;

            GameObject newSlot = Instantiate(questItemPrefab, questContainer);
            QuestUIItem uiItem = newSlot.GetComponent<QuestUIItem>();

            if (uiItem != null)
            {
                int currentProgress = QuestManager.Instance.GetQuestProgress(quest.questID);
                int currentStage = QuestManager.Instance.GetQuestStage(quest.questID);
                bool isCompleted = QuestManager.Instance.IsQuestCompleted(quest.questID);

                uiItem.SetupUI(quest, currentProgress, currentStage, isCompleted);
                spawnedItems.Add(uiItem);
            }
        }
    }
}