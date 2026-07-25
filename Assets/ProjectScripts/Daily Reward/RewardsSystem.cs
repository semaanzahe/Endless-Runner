using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardsSystem : MonoBehaviour
{
    public static RewardsSystem Instance;

    [Header("UI References")]
    [SerializeField] private TMP_Text timerText; // Drag your UI Text component here in Inspector

    private List<GameObject> RewardsList = new List<GameObject>();
    private GameObject currentReward;

    private int currentRewardDay = 0;
    private string savedTimeStamp;
    private bool canClaim = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RewardsLayout layout = GetComponent<RewardsLayout>();
        if (layout != null)
        {
            RewardsList = layout.itemPrefabs;
        }

        LoadSavedProgress();
        canClaim = CanClaimToday(savedTimeStamp);
        GetCurrentReward();
        UpdateUI();
    }

    private void Update()
    {
        // Only run the timer countdown when rewards are on cooldown
        if (!canClaim && !string.IsNullOrEmpty(savedTimeStamp))
        {
            UpdateCountdownTimer();
        }
    }

    private void UpdateCountdownTimer()
    {
        if (DateTime.TryParse(savedTimeStamp, out DateTime lastClaim))
        {
            // Target time is 24 hours after the last claim
            DateTime nextClaimTime = lastClaim.AddHours(24);
            TimeSpan timeRemaining = nextClaimTime - DateTime.Now;

            if (timeRemaining.TotalSeconds > 0)
            {
                // Format display as HH:MM:SS
                if (timerText != null)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", 
                        timeRemaining.Hours, 
                        timeRemaining.Minutes, 
                        timeRemaining.Seconds);
                }
            }
            else
            {
                // Timer finished! Unlock claim state automatically
                canClaim = true;
                if (timerText != null)
                {
                    timerText.text = "Ready to Claim!";
                }
                UpdateUI();
            }
        }
    }

    private void GetCurrentReward()
    {
        if (RewardsList == null || RewardsList.Count == 0) return;

        // Reset streak to Day 1 (index 0) if player completed all 7 days
        if (currentRewardDay >= RewardsList.Count)
        {
            currentRewardDay = 0;
        }

        currentReward = RewardsList[currentRewardDay];
    }

    public bool CanClaimToday(string timeStamp)
    {
        if (string.IsNullOrEmpty(timeStamp)) 
            return true; // First time player

        if (!DateTime.TryParse(timeStamp, out DateTime lastClaim))
            return true; // Fallback if string parse fails

        DateTime now = DateTime.Now;

        // Check if 24 full hours have passed
        TimeSpan timePassed = now - lastClaim;
        return timePassed.TotalHours >= 24;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void UpdateUI()
    {
        for (int i = 0; i < RewardsList.Count; i++)
        {
            GameObject rewardObj = RewardsList[i];
            if (rewardObj == null) continue;

            Button rewardBtn = rewardObj.GetComponentInChildren<Button>(true);
            if (rewardBtn == null) continue;

            // Re-attach OnClick
            rewardBtn.onClick.RemoveAllListeners();
            rewardBtn.onClick.AddListener(OnClaimButtonClicked);

            GameObject activeHighlight = GetChildWithTag(rewardObj, "Active_HighLight");
            GameObject claimedHighlight = GetChildWithTag(rewardObj, "Claimed_HighLight");

            // PAST DAYS (Already claimed previously)
            if (i < currentRewardDay - 1)
            {
                rewardBtn.interactable = false;
                if (activeHighlight != null) activeHighlight.SetActive(false);
                if (claimedHighlight != null) claimedHighlight.SetActive(true);
            }
            // CURRENT/MOST RECENT DAY
            else if (i == currentRewardDay - 1 && !canClaim)
            {
                // Just claimed today! Show checkmark, disable button
                rewardBtn.interactable = false;
                if (activeHighlight != null) activeHighlight.SetActive(false);
                if (claimedHighlight != null) claimedHighlight.SetActive(true);
            }
            // ACTIVE DAY READY TO CLAIM
            else if (i == currentRewardDay && canClaim)
            {
                // Ready to claim today!
                rewardBtn.interactable = true;
                if (activeHighlight != null) activeHighlight.SetActive(true);
                if (claimedHighlight != null) claimedHighlight.SetActive(false);
            }
            // FUTURE DAYS (Locked)
            else
            {
                rewardBtn.interactable = false;
                if (activeHighlight != null) activeHighlight.SetActive(false);
                if (claimedHighlight != null) claimedHighlight.SetActive(false);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
/// Searches all children recursively for a tag (including inactive objects).
/// </summary>
private GameObject GetChildWithTag(GameObject parent, string tag)
{
    Transform[] children = parent.GetComponentsInChildren<Transform>(true);
    foreach (Transform child in children)
    {
        if (child.CompareTag(tag))
        {
            return child.gameObject;
        }
    }
    return null;
}

// ReSharper disable Unity.PerformanceAnalysis
public void OnClaimButtonClicked()
{
    if (!canClaim || currentReward == null) return;

    // 1. Give reward based on type
    Reward rewardComponent = currentReward.GetComponentInChildren<Reward>(true);
    if (rewardComponent != null && rewardComponent.reward != null)
    {
        switch (rewardComponent.reward.rewardType)
        {
            case RewardType.Gold:
                Serializator.instance.AddCoinsAndSave(rewardComponent.reward.rewardAmount);
                break;
        }
    }

    // 2. Lock claiming immediately for the rest of today
    canClaim = false; 

    // 3. Increment the claimed day counter (Day 1 claimed -> now progress index is 1)
    currentRewardDay++;
    savedTimeStamp = DateTime.Now.ToString();

    // Schedule notification for exactly 24 hours from now!
    DateTime nextRewardTime = DateTime.Now.AddHours(24);
    NotificationHandler.ScheduleRewardNotification(nextRewardTime);
    
    // 4. Save updated progress to JSON
    if (Serializator.instance != null)
    {
        Serializator.instance.SaveRewardProgress(currentRewardDay, savedTimeStamp);
    }

    // 5. Update active reward reference and refresh UI
    GetCurrentReward();
    UpdateUI();
}

    private void LoadSavedProgress()
    {
        string json = SaveSystem.Load();
        if (!string.IsNullOrEmpty(json))
        {
            SerializedData data = JsonUtility.FromJson<SerializedData>(json);
            currentRewardDay = data.lastClaimedDay;
            savedTimeStamp = data.lastClaimTimeStamp;

            Debug.Log($"[RewardsSystem] Loaded Day: {currentRewardDay}, Loaded Timestamp: {savedTimeStamp}");
        }
        else
        {
            Debug.LogWarning("[RewardsSystem] Save file is empty or missing!");
        }
    }
}