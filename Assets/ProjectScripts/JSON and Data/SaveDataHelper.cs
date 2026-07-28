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

            // Set active profile using saved name and slot index
            SaveSystem.SetActiveProfile(existingData.profileName, slotIndex);

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
}