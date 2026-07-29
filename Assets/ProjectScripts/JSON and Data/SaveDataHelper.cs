using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SaveDataHelper : MonoBehaviour
{
    [Header("UI Containers")]
    [SerializeField] private GameObject createProfilePanel;
    [SerializeField] private TMP_InputField profileNameInput;
    [SerializeField] private Button confirmCreateButton;

    [Header("Slot Setup")]
    [SerializeField] private Transform slotsParent;

    private int selectedProfileIndex = -1;
    
    private string defaultProfileName = "Astronaut";
    private int defaultProfileNumber = 0;

    private void Start()
    {
        if (createProfilePanel != null)
            createProfilePanel.SetActive(false);

        if (confirmCreateButton != null)
            confirmCreateButton.onClick.AddListener(OnConfirmCreateProfile);

        SetupSlotButtons();
    }

    private void SetupSlotButtons()
    {
        if (slotsParent == null) return;

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            Transform slotChild = slotsParent.GetChild(i);
            Button slotButton = slotChild.GetComponent<Button>();

            if (slotButton != null)
            {
                int buttonIndex = i;
                slotButton.onClick.RemoveAllListeners();
                slotButton.onClick.AddListener(() => OnSlotButtonClicked(buttonIndex));

                UpdateSlotButtonUI(slotChild, buttonIndex);
            }
        }
    }

    private void OnSlotButtonClicked(int slotIndex)
    {
        selectedProfileIndex = slotIndex;
        SerializedData existingData = GetSaveDataForSlot(slotIndex);

        if (existingData != null)
        {
            // --- SAVE FILE EXISTS ---
            Debug.Log($"Loading save file for '{existingData.profileName}'...");

            if (Serializator.instance != null)
            {
                Serializator.instance.currentProfileNumber = slotIndex; 
                Serializator.instance.currentProfileName = profileNameInput.text;  
            }
            
            // Then set active profile on SaveSystem:
            SaveSystem.SetActiveProfile(profileNameInput.text, slotIndex);

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            // --- NO SAVE FILE EXISTS ---
            if (profileNameInput != null)
                profileNameInput.text = "";

            if (createProfilePanel != null)
                createProfilePanel.SetActive(true);
        }
    }

    private void OnConfirmCreateProfile()
    {
        if (selectedProfileIndex == -1) return;

        string enteredName = profileNameInput != null ? profileNameInput.text.Trim() : "";

        if (string.IsNullOrEmpty(enteredName))
        {
            enteredName = $"Astronaut_{selectedProfileIndex}";
        }

        // Creates file as Application.persistentDataPath + "/Saves/" + enteredName + ".json"
        SaveSystem.CreateNewSave(selectedProfileIndex, enteredName);

        if (createProfilePanel != null)
            createProfilePanel.SetActive(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Searches the /Saves/ folder for a JSON file where profileNumber matches the slot index
    private SerializedData GetSaveDataForSlot(int slotIndex)
    {
        SaveSystem.Init();

        if (!Directory.Exists(SaveSystem.SAVE_FOLDER)) return null;

        string[] files = Directory.GetFiles(SaveSystem.SAVE_FOLDER, "*.json");

        foreach (string filePath in files)
        {
            string json = File.ReadAllText(filePath);
            SerializedData data = JsonUtility.FromJson<SerializedData>(json);

            if (data != null && data.profileNumber == slotIndex)
            {
                return data;
            }
        }

        return null;
    }

    private void UpdateSlotButtonUI(Transform slotTransform, int slotIndex)
    {
        SerializedData data = GetSaveDataForSlot(slotIndex);
        TMP_Text buttonText = slotTransform.GetComponentInChildren<TMP_Text>();

        if (buttonText != null)
        {
            if (data != null)
            {
                buttonText.text = $"{data.profileName} (Slot {slotIndex})";
            }
            else
            {
                buttonText.text = $"[ Empty Slot {slotIndex} ]";
            }
        }
    }
    public void OnTapToPlayPressed()
    {
        // Check if any save files exist in the folder
        bool saveExists = SaveSystem.HasAnySaves(); // Or check Directory.GetFiles(SaveSystem.SAVE_FOLDER, "*.json").Length > 0

        if (!saveExists)
        {
            // NO SAVES FOUND: Set up and create Slot 0 as a brand-new save profile
            SaveSystem.CreateNewSave(defaultProfileNumber, defaultProfileName);

            if (Serializator.instance != null)
            {
                Serializator.instance.currentProfileName = defaultProfileName;
                Serializator.instance.currentProfileNumber = defaultProfileNumber;
            
                // Explicitly create the initial save file so future loads find it
                Serializator.instance.SaveDataWithoutScreenshot();
            }

            Debug.Log($"[TapToPlay] No save found. Created default save profile: {defaultProfileName} at Slot {defaultProfileNumber}");
        }
        else
        {
            // SAVE EXISTS: Load Slot 0 (the first save profile)
            SaveSystem.SetActiveProfile(defaultProfileName, defaultProfileNumber);

            if (Serializator.instance != null)
            {
                Serializator.instance.currentProfileName = defaultProfileName;
                Serializator.instance.currentProfileNumber = defaultProfileNumber;
                Serializator.instance.DeserializeData(); // Load the existing data into memory
            }

            Debug.Log($"[TapToPlay] Loaded existing save slot: {defaultProfileName}");
        }

        // Launch into your gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
    }

    public void CloseCreateProfilePanel()
    {
        createProfilePanel.SetActive(false);
    }
}