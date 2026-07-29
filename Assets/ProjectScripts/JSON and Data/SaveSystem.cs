using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
    
    // Tracks the current file name 
    private static string currentSaveFileName = "Astronaut";
    private static int currentProfileIndex = 0;
    
    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    // Call this using the custom profile name!
    public static void SetActiveProfile(string profileName, int profileIndex)
    {
        currentProfileIndex = profileIndex;
        // Replaces any invalid file name characters with an underscore for safety
        string safeFileName = string.Join("_", profileName.Split(Path.GetInvalidFileNameChars()));
        currentSaveFileName = $"{safeFileName}.json";
    }

    public static void CreateNewSave(int profileNumber, string profileName)
    {
        Init();
        SetActiveProfile(profileName, profileNumber);

        SerializedData newData = new SerializedData
        {
            profileNumber = profileNumber,
            profileName = profileName,
            totalCoins = 0,
            highestScore = 0,
            scoreMultiplier = 1,
            lastClaimedDay = 0,
            lastClaimTimeStamp = ""
        };

        string json = JsonUtility.ToJson(newData, true);
        Save(json);

        Debug.Log($"[SaveSystem] Created file at: {Path.Combine(SAVE_FOLDER, currentSaveFileName)}");
    }

    public static void SaveThumbnail(byte[] imageBytes)
    {
        Init();
        // Uses currentProfileIndex for thumbnail tracking
        string imagePath = Path.Combine(SAVE_FOLDER, $"profile_{currentProfileIndex}.png");
        File.WriteAllBytes(imagePath, imageBytes);
    }
    
    public static void Save(string saveString)
    {
        Init();
        string fullPath = Path.Combine(SAVE_FOLDER, currentSaveFileName);
        File.WriteAllText(fullPath, saveString);
    }

    public static string Load()
    {
        Init();
        string fullPath = Path.Combine(SAVE_FOLDER, currentSaveFileName);

        if (File.Exists(fullPath))
        {
            return File.ReadAllText(fullPath);
        }
        return null;
    }
    public static bool HasAnySaves()
    {
        // 1. Ensure SAVE_FOLDER string is valid
        if (string.IsNullOrEmpty(SAVE_FOLDER))
        {
            return false;
        }

        // 2. Check if directory exists
        if (!Directory.Exists(SAVE_FOLDER))
        {
            return false;
        }

        // 3. Search for any .json save files inside the directory
        string[] saveFiles = Directory.GetFiles(SAVE_FOLDER, "*.json");
        
        return saveFiles.Length > 0;
    }
    
}