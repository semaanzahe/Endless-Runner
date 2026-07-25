using System.IO;
using UnityEngine;
using FxResources.System.IO;

public static class SaveSystem
{
    public static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
    
    private static string currentSaveFileName = "save_1.json";
    private static int currentProfileIndex = 1;
    
    public static void Init()
    {
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
        
    }
    // The UI buttons call this to change the active file target
    public static void SetActiveProfile(int profileNumber)
    {
        currentSaveFileName = $"profile_{profileNumber}.json";
    }

    public static void SaveThumbnail(byte[] imageBytes)
    {
        Init();
        string imagePath = Path.Combine(SAVE_FOLDER, $"profile_{currentProfileIndex}.png");
        File.WriteAllBytes(imagePath, imageBytes);
        Debug.Log($"Thumbnail saved to: {imagePath}");
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
    
    public static void SaveClaimedDayAndTimestamp(int day, string timestamp)
    {
        string json = Load();
        SerializedData data = !string.IsNullOrEmpty(json) 
            ? JsonUtility.FromJson<SerializedData>(json) 
            : new SerializedData();

        data.lastClaimedDay = day;
        data.lastClaimTimeStamp = timestamp;

        string updatedJson = JsonUtility.ToJson(data, true);
        Save(updatedJson);
    }
    
}
