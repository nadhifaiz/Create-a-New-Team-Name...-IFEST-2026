using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("Volume Sliders")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private void Start()
    {
        float bgmVal = DataManager.Instance != null ? DataManager.Instance.GetBGMVolume() : 0.5f;
        float sfxVal = DataManager.Instance != null ? DataManager.Instance.GetSFXVolume() : 0.5f;

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = bgmVal;
            OnMusicSliderChanged(bgmVal);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = sfxVal;
            OnSFXSliderChanged(sfxVal);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    public void OnMusicSliderChanged(float value)
    {
        GameEvents.onValueChangeBGM?.Invoke(value);
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SetBGMVolume(value);
        }
    }

    public void OnSFXSliderChanged(float value)
    {
        GameEvents.onValueChangeSFX?.Invoke(value);
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SetSFXVolume(value);
        }
    }
}