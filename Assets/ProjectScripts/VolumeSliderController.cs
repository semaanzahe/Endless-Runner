using UnityEngine;
using UnityEngine.UI;

public class VolumeSliderController : MonoBehaviour
{
    [Header("UI Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void OnEnable()
    {
        // 1. Configure sliders to work on a 0 to 1 scale
        musicSlider.minValue = 0.0001f; // Avoid absolute 0 for Log10 math safety
        musicSlider.maxValue = 1f;
        
        sfxSlider.minValue = 0.0001f;
        sfxSlider.maxValue = 1f;

        // 2. Set sliders to match currently saved values on open
        musicSlider.value = PlayerPrefs.GetFloat("SavedMusicVol", 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat("SavedSFXVol", 0.75f);

        // 3. Listen for when user drags the slider handle
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
    }

    void OnDisable()
    {
        // Clean up listeners when the menu closes
        musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
    }

    private void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    private void OnSFXSliderChanged(float value)
    {
        AudioManager.Instance.SetSFXVolume(value);
    }
}
