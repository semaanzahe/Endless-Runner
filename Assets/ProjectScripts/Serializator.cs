using System.Collections;
using System.IO;
using UnityEngine;

public class Serializator : MonoBehaviour
{
    
    public static  Serializator instance;
    
    public string currentProfileName = "Astronaut_1";
    public int currentProfileNumber = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
       // SerializeData();
       DeserializeData();
    }

    public void SerializeData()
    {
        StartCoroutine(CaptureAndSaveRoutine());
    }

    private IEnumerator CaptureAndSaveRoutine()
    {
        // 1. Wait until the very end of the frame so UI and gameplay elements are fully rendered
        yield return new WaitForEndOfFrame();

        // 2. Capture the screen as a Texture2D
        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

        // 3. Encode the texture into PNG format bytes
        byte[] pngBytes = screenShot.EncodeToPNG();

        // 4. Clean up the texture memory to prevent memory leaks
        Destroy(screenShot);

        // 5. Save the image data using the save system
        SaveSystem.SaveThumbnail(pngBytes);

        // 6. Now save your standard JSON progression data
        SerializedData data = new SerializedData
        {
            profileName = currentProfileName,
            profileNumber = currentProfileNumber,
            highestScore = Hud.Instance.highScore,
            totalCoins = Hud.Instance.totalCoins
        };

        string json = JsonUtility.ToJson(data);
        SaveSystem.Save(json);
        
        Debug.Log("Profile and Thumbnail Saved Successfully.");
    }

    public void DeserializeData()
    {
        string saveString = SaveSystem.Load();
        if (saveString != null)
        {
            SerializedData data = JsonUtility.FromJson<SerializedData>(saveString);
            currentProfileName = data.profileName;
            currentProfileNumber = data.profileNumber;
            Hud.Instance.highScore = data.highestScore;
            Hud.Instance.totalCoins = data.totalCoins;
        }
        else
        {
            Hud.Instance.highScore = 0;
            Hud.Instance.totalCoins = 0;
        }
    }
}
