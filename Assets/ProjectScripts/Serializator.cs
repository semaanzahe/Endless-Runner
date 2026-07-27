using System.Collections;
using System.IO;
using UnityEngine;

public class Serializator : MonoBehaviour
{
    public static Serializator instance;
    
    public string currentProfileName = "Astronaut_1";
    public int currentProfileNumber = 1;

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
            if (Hud.Instance != null)
            {
                Hud.Instance.highScore = data.highestScore;
                Hud.Instance.totalCoins = data.totalCoins;
            }
        }
        else
        {
            if (Hud.Instance != null)
            {
                Hud.Instance.highScore = 0;
                Hud.Instance.totalCoins = 0;
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
    public void CreateNewProfile(int profileNumber = 1, string profileName = "Astronaut")
    {
        // 1. Tell SaveSystem to build the new JSON file
        SaveSystem.CreateNewSave(profileNumber, profileName);

        // 2. Set current runtime variables to match
        currentProfileNumber = profileNumber;
        currentProfileName = $"{profileName}_{profileNumber}";

        // 3. Load that fresh data into your HUD/Game
        DeserializeData();
    }
    
    // FIXED: Reads existing save file to keep lastClaimedDay & lastClaimTimeStamp intact
    public void SaveDataWithoutScreenshot()
    {
        string json = SaveSystem.Load();
        SerializedData data = !string.IsNullOrEmpty(json) 
            ? JsonUtility.FromJson<SerializedData>(json) 
            : new SerializedData();

        data.profileName = currentProfileName;
        data.profileNumber = currentProfileNumber;
        if (Hud.Instance != null)
        {
            data.highestScore = Hud.Instance.highScore;
            data.totalCoins = Hud.Instance.totalCoins;
        }

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