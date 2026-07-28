using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image questIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressBar;
    
    [Header("Reward & Claim")]
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private Image rewardIcon;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject completedCheckmark;

    private QuestSO currentQuest;
    private int currentStageIndex;
    private int currentProgress;

    public void SetupUI(QuestSO quest, int progress, int stageIndex, bool isCompleted)
    {
        currentQuest = quest;
        currentProgress = progress;
        currentStageIndex = stageIndex;

        // 1. Basic Info
        if (titleText != null) titleText.text = quest.title;
        if (descriptionText != null) descriptionText.text = quest.description;
        if (questIcon != null && quest.icon != null) questIcon.sprite = quest.icon;

        // Check if all stages are completed
        if (stageIndex >= quest.stages.Count || isCompleted)
        {
            ShowCompletedState();
            return;
        }

        // 2. Stage Goal & Progress
        MissionStage currentStage = quest.stages[stageIndex];
        int target = currentStage.targetAmount;

        if (progressText != null) 
            progressText.text = $"{currentProgress} / {target}";

        if (progressBar != null)
        {
            progressBar.maxValue = target;
            progressBar.value = Mathf.Min(currentProgress, target);
        }

        // 3. Rewards & Claim Button
        if (rewardAmountText != null) 
            rewardAmountText.text = $"x{currentStage.rewardAmount}";

        bool canClaim = currentProgress >= target;
        
        if (claimButton != null)
        {
            claimButton.gameObject.SetActive(true);
            claimButton.interactable = canClaim;
            claimButton.onClick.RemoveAllListeners();
            claimButton.onClick.AddListener(OnClaimClicked);
        }

        if (completedCheckmark != null) 
            completedCheckmark.SetActive(false);
    }

    private void ShowCompletedState()
    {
        if (progressText != null) progressText.text = "COMPLETED!";
        if (progressBar != null) progressBar.value = progressBar.maxValue;
        if (claimButton != null) claimButton.gameObject.SetActive(false);
        if (completedCheckmark != null) completedCheckmark.SetActive(true);
    }

    private void OnClaimClicked()
    {
        // Tells QuestManager to process the reward and increment the stage
        QuestManager.Instance?.ClaimReward(currentQuest.questID);
    }
}