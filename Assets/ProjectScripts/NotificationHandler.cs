using System;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
#endif

public class NotificationHandler : MonoBehaviour
{
    
    private void Start()
    {
#if UNITY_ANDROID
        // Check if the app was opened by tapping a notification
        var intentData = AndroidNotificationCenter.GetLastNotificationIntent();

        if (intentData != null)
        {
            Debug.Log($"[Notification] App opened via notification channel: {intentData.Channel}");

            // Verify it was our daily rewards notification
            if (intentData.Channel == "daily_rewards_channel")
            {
                CanvasManager.instance.OpenDailyRewards();
            }
        }
#endif
    }
    public static void ScheduleRewardNotification(DateTime targetTime)
    {
#if UNITY_ANDROID
        // 1. Setup Notification Channel (Required for Android)
        var channel = new AndroidNotificationChannel()
        {
            Id = "daily_rewards_channel",
            Name = "Daily Rewards Channel",
            Importance = Importance.High,
            Description = "Notifications for Daily Claimable Rewards",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);

        // 2. Clear old scheduled notifications so they don't stack up
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        // 3. Create the Notification
        var notification = new AndroidNotification();
        notification.Title = "Your Daily Reward is Ready! 🎁";
        notification.Text = "Jump back in to claim your daily coins!";
        notification.FireTime = targetTime; // Target 24 hours after claim
        notification.SmallIcon = "icon_small"; // Needs to match an icon in Android settings or default

        // 4. Send to Android OS
        AndroidNotificationCenter.SendNotification(notification, "daily_rewards_channel");
        Debug.Log($"[Notification] Scheduled for: {targetTime}");
#endif
    }
}