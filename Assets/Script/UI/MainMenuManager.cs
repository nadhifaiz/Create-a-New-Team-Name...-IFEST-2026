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
    [SerializeField] SoundID sfx_HitPlay;

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
        AudioSystem.Instance.PlayAudio(sfx_HitPlay);
        AudioSystem.Instance.StopBGM();
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        AudioSystem.Instance.PlayAudio(sfx_HitPlay);
        Debug.Log("Quit game");
        Application.Quit();
    }

    public void PlayButtonClickSFX()
    {
        AudioSystem.Instance.PlayAudio(sfx_HitPlay);
    }

    public void OpenPopUp(GameObject gameobject)
    {
        AudioSystem.Instance.PlayAudio(sfx_HitPlay);
        gameobject.SetActive(true);
    }

    public void ClosePopup(GameObject gameobject)
    {
        AudioSystem.Instance.PlayAudio(sfx_HitPlay);
        gameobject.SetActive(false);
    }
}