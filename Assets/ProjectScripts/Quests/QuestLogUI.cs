using System.Collections.Generic;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform questContainer; // Assign QuestsLayout here
    [SerializeField] private GameObject questItemPrefab; // Assign your Quest Prefab here

    private List<QuestUIItem> spawnedItems = new List<QuestUIItem>();

    private void OnEnable()
    {
        // Subscribe to QuestManager events
        QuestManager.OnQuestProgressUpdated += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        // Always unsubscribe on disable to avoid memory leaks!
        QuestManager.OnQuestProgressUpdated -= RefreshUI;
    }

    public void RefreshUI()
    {
        if (QuestManager.Instance == null) return;

        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();

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