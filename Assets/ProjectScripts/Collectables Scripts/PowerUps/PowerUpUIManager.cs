using System.Collections.Generic;
using UnityEngine;

public class PowerUpUIManager : MonoBehaviour
{
    public static PowerUpUIManager Instance;

    [Header("UI Prefab & Parent Setup")]
    [SerializeField] private GameObject powerUpDisplayPrefab;
    [SerializeField] private Transform displayContainer;

    // Dictionary tracking active UI instances by their PowerUp type enum
    private Dictionary<PowerUpsEnum, PowerUpDisplayUI> activePowerUpUIs = new Dictionary<PowerUpsEnum, PowerUpDisplayUI>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowPowerUpUI(PowerUpsEnum type, string powerUpName, float duration)
    {
        if (powerUpDisplayPrefab == null || displayContainer == null) return;

        // 1. DUPLICATE CHECK: If this power-up type is already active, force-remove the old UI card
        if (activePowerUpUIs.TryGetValue(type, out PowerUpDisplayUI existingUI))
        {
            if (existingUI != null)
            {
                existingUI.ForceRemove();
            }
            activePowerUpUIs.Remove(type);
        }

        // 2. Instantiate new UI card
        GameObject displayObj = Instantiate(powerUpDisplayPrefab, displayContainer);
        PowerUpDisplayUI displayScript = displayObj.GetComponent<PowerUpDisplayUI>();

        if (displayScript != null)
        {
            // Initialize with the enum type so it knows its identity
            displayScript.Initialize(type, powerUpName, duration);
            
            // Register new UI card into the dictionary
            activePowerUpUIs[type] = displayScript;
        }
    }

    // Called by PowerUpDisplayUI when the timer runs out naturally
    public void RemoveFromDictionary(PowerUpsEnum type)
    {
        if (activePowerUpUIs.ContainsKey(type))
        {
            activePowerUpUIs.Remove(type);
        }
    }
}