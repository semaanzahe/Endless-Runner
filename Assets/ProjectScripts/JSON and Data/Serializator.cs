using System.Collections;
using System.IO;
using UnityEngine;

public class Serializator : MonoBehaviour
{
    public static Serializator instance;
    
    public string currentProfileName = "Astronaut";
    public int currentProfileNumber = 0;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DeserializeData();
    }

    public void SerializeData()
    {
        StartCoroutine(CaptureAndSaveRoutine());
    }

    private IEnumerator CaptureAndSaveRoutine()
    {
        yield return new WaitForEndOfFrame();

        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] pngBytes = screenShot.EncodeToPNG();
        Destroy(screenShot);

        SaveSystem.SaveThumbnail(pngBytes);

        // Save JSON safely preserving existing fields
        SaveDataWithoutScreenshot();
        
        Debug.Log("Profile and Thumbnail Saved Successfully.");
    }

    public void DeserializeData()
    {
        string saveString = SaveSystem.Load();
        if (!string.IsNullOrEmpty(saveString))
        {
            SerializedData data = JsonUtility.FromJson<SerializedData>(saveString);
        
            currentProfileName = data.profileName;
            currentProfileNumber = data.profileNumber;

            SaveSystem.SetActiveProfile(currentProfileName, currentProfileNumber);

            if (Hud.Instance != null && data != null)
            {
                Hud.Instance.highScore = data.highestScore;
                Hud.Instance.totalCoins = data.totalCoins;
                Hud.Instance.scoreMultiplier = data.scoreMultiplier;
            }
            if (QuestManager.Instance != null && data != null)
            {
                QuestManager.Instance.LoadSaveData(data.questProgressList);
            }
        }
    }

    
    private void OnApplicationQuit()
    {
        SaveDataWithoutScreenshot();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveDataWithoutScreenshot();
        }
    }
    
    
    public void SaveDataWithoutScreenshot()
    {
        // 1. Check if a save file exists
        string json = SaveSystem.Load();

        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("[Serializator] Save attempt aborted: No existing save file found.");
            return; // Safe exit—does nothing if no save file exists
        }

        // 2. Deserialize existing save file data
        SerializedData data = JsonUtility.FromJson<SerializedData>(json);

        // 3. Update active profile identifiers
        data.profileName = currentProfileName;
        data.profileNumber = currentProfileNumber;

        // 4. Update HUD stats if available
        if (Hud.Instance != null)
        {
            data.highestScore = Hud.Instance.highScore;
            data.totalCoins = Hud.Instance.totalCoins;
            data.scoreMultiplier = Hud.Instance.scoreMultiplier;
        }

        // 5. Update quest progress if available
        if (QuestManager.Instance != null)
        {
            data.questProgressList = QuestManager.Instance.GetSaveData();
        }

        // 6. Save updated data back to disk
        SaveSystem.Save(JsonUtility.ToJson(data, true));
    }
    
    public void AddCoinsAndSave(int amount)
    {
        string json = SaveSystem.Load();
        SerializedData data = !string.IsNullOrEmpty(json) 
            ? JsonUtility.FromJson<SerializedData>(json) 
            : new SerializedData();

        data.totalCoins += amount;

        SaveSystem.Save(JsonUtility.ToJson(data, true));
    }

    public void SaveRewardProgress(int claimedDay, string timeStamp)
    {
        string json = SaveSystem.Load();
        SerializedData data = !string.IsNullOrEmpty(json) 
            ? JsonUtility.FromJson<SerializedData>(json) 
            : new SerializedData();

        data.lastClaimedDay = claimedDay;
        data.lastClaimTimeStamp = timeStamp;

        SaveSystem.Save(JsonUtility.ToJson(data, true));
        Debug.Log($"[Serializator] Saved Day: {claimedDay} | Time: {timeStamp}");
    }
    
    
}