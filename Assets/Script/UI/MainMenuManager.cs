using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ami.BroAudio;

public class MainMenuManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Nama scene yang mau dibuka saat tombol Play ditekan")]
    public string gameSceneName = "SampleScene";

    [Header("BroAudio - Sound yang dipakai di Main Menu")]
    [Tooltip("SFX yang diputar saat tombol di-klik (Play/Quit/dll)")]
    [SerializeField] SoundID buttonClickSFX;

    [Header("Volume Sliders")]
    [Tooltip("Slider untuk volume musik (BGM)")]
    public Slider musicVolumeSlider;

    [Tooltip("Slider untuk volume sound effect (SFX)")]
    public Slider sfxVolumeSlider;

    void Start()
    {
        AudioSystem.Instance.PlayAudio(AudioSystem.Instance.bgm_MainMenu);

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", AudioSystem.Instance.defaultBGM);
            OnMusicSliderChanged(musicVolumeSlider.value);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", AudioSystem.Instance.defaultSFX);
            OnSFXSliderChanged(sfxVolumeSlider.value);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    public void OnMusicSliderChanged(float value)
    {
        GameEvents.onValueChangeBGM?.Invoke(value);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXSliderChanged(float value)
    {
        GameEvents.onValueChangeSFX?.Invoke(value);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    public void PlayGame()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
        AudioSystem.Instance.StopBGM();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void PlayButtonClickSFX()
    {
        AudioSystem.Instance.PlayAudio(buttonClickSFX);
    }

    public void OpenPopUp(GameObject gameobject)
    {
        gameobject.SetActive(true);
    }

    public void ClosePopup(GameObject gameobject)
    {
        gameobject.SetActive(false);
    }
}