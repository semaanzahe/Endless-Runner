using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RewardsLayout : MonoBehaviour
{
    [SerializeField] internal List<GameObject> itemPrefabs = new List<GameObject>();

    private void Awake()
    {
        // At runtime (Play Mode), instantiate any raw prefab assets in the list into real UI scene objects
        InstantiatePrefabsForRuntime();
    }

    private void InstantiatePrefabsForRuntime()
    {
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            GameObject slot = itemPrefabs[i];
            if (slot == null) continue;

            // If it's a raw Prefab asset, instantiate it in the scene properly
            if (!slot.scene.IsValid())
            {
                GameObject instance = Instantiate(slot, transform);
                itemPrefabs[i] = instance;
            }
            else if (slot.transform.parent != transform)
            {
                slot.transform.SetParent(transform, false);
            }
        }

        RefreshDayTexts();
    }

    // Call this at runtime to add a new reward slot dynamically
    public void AddChild(GameObject newObject)
    {
        if (newObject == null) return;

        GameObject instance = newObject.scene.IsValid() ? newObject : Instantiate(newObject, transform);
        instance.transform.SetParent(transform, false);

        if (!itemPrefabs.Contains(instance))
        {
            itemPrefabs.Add(instance);
        }

        RefreshDayTexts();
    }

    // Updates text labels ("Day 1", "Day 2", etc.) without instantiating objects
    public void RefreshDayTexts()
    {
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            GameObject slot = itemPrefabs[i];
            if (slot == null) continue;

            Transform rewardChild = FindChildWithTag(slot.transform, "Reward");
            if (rewardChild != null)
            {
                Transform dayTextChild = FindChildWithTag(rewardChild, "Reward_Day");
                if (dayTextChild != null)
                {
                    TextMeshProUGUI textComp = dayTextChild.GetComponent<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = $"Day {i + 1}";
                    }
                }
            }
        }
    }

    // Helper function to find direct children by tag
    private Transform FindChildWithTag(Transform parent, string tag)
    {
        foreach (Transform child in parent)
        {
            if (child.CompareTag(tag))
            {
                return child;
            }
        }
        return null;
    }

#if UNITY_EDITOR
    // Only update day labels in the Editor — NEVER call Instantiate inside OnValidate!
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                RefreshDayTexts();
            }
        };
    }
#endif
}