using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpDisplayUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI powerUpNameText;
    [SerializeField] private Slider timerSlider;

    private PowerUpsEnum currentType;
    private Coroutine timerCoroutine;

    public void Initialize(PowerUpsEnum type, string powerUpName, float duration)
    {
        currentType = type;

        if (powerUpNameText != null)
        {
            powerUpNameText.text = powerUpName;
        }

        timerCoroutine = StartCoroutine(TimerRoutine(duration));
    }

    private IEnumerator TimerRoutine(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            if (timerSlider != null)
            {
                timerSlider.value = Mathf.Clamp01(1f - (elapsedTime / duration));
            }

            yield return null;
        }

        // Timer ended naturally -> remove from activePowerUpUIs dictionary and destroy
        if (PowerUpUIManager.Instance != null)
        {
            PowerUpUIManager.Instance.RemoveFromDictionary(currentType);
        }

        Destroy(gameObject);
    }

    // Called when a duplicate power-up replaces this active one early
    public void ForceRemove()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        Destroy(gameObject);
    }
}