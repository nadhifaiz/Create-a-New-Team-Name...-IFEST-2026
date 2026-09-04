using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Ami.BroAudio; //[cite: 1]

public class MainMenuManager : MonoBehaviour
{
    public static bool cameFromCutscene = false;
    public static bool openLevelSelectOnLoad = false;

    [Header("Dynamic Background")]
    [SerializeField] private Image mainMenuBackgroundImage;
    [SerializeField] private Sprite[] levelBackgrounds;

    [Header("Scene Navigation")]
    [SerializeField] private string cutsceneSceneName = "Cutscene";

    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Main Menu BGM")]
    [SerializeField] private SoundID bgm_MainMenu; //[cite: 1]


    private void Start()
    {
        UpdateMainMenuBackground();

        // Jika datang dari cutscene (alur tutorial)
        if (cameFromCutscene)
        {
            cameFromCutscene = false;
            bool tutorialDone = DataManager.Instance != null && DataManager.Instance.HasCompletedTutorial();

            if (!tutorialDone) ShowTutorialPanel();
            else ShowLevelSelection();
        }
        // Jika datang dari tombol Next Level di in-game
        else if (openLevelSelectOnLoad)
        {
            openLevelSelectOnLoad = false;
            ShowLevelSelection();
        }
        else
        {
            ShowMainPanel();
        }

        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.PlayBGM(AudioSystem.Instance.bgm_MainMenu);
        }
    }

    private void UpdateMainMenuBackground()
    {
        if (mainMenuBackgroundImage == null || levelBackgrounds == null || levelBackgrounds.Length == 0) return;

        int lastLevel = 1;
        if (DataManager.Instance != null)
        {
            lastLevel = DataManager.Instance.GetLastPlayedLevel();
        }

        // Konversi levelIndex (1, 2, 3) ke array index (0, 1, 2)
        int bgIndex = Mathf.Clamp(lastLevel - 1, 0, levelBackgrounds.Length - 1);
        mainMenuBackgroundImage.sprite = levelBackgrounds[bgIndex];
    }

    public void PlayGame()
    {
        PlayButtonClickSFX();
        // Hapus StopBGM() agar audio tidak mati saat pindah ke Cutscene
        SceneManager.LoadScene(cutsceneSceneName);
    }

    public void OnFinishTutorial()
    {
        PlayButtonClickSFX(); //[cite: 1]
        if (DataManager.Instance != null) DataManager.Instance.CompleteTutorial();
        ShowLevelSelection();
    }

    public void ShowTutorialPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuBackgroundImage != null) mainMenuBackgroundImage.gameObject.SetActive(true);
    }

    public void ShowLevelSelection()
    {
        if (mainPanel != null) mainPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        if (mainMenuBackgroundImage != null) mainMenuBackgroundImage.gameObject.SetActive(true);
    }

    public void ShowMainPanel()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuBackgroundImage != null) mainMenuBackgroundImage.gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        PlayButtonClickSFX(); //[cite: 1]
        Application.Quit(); //[cite: 1]
    }

    public void PlayButtonClickSFX()
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.PlaySfx(AudioSystem.Instance.sfx_HitPlay);
        }
    }

    public void OpenPopUp(GameObject popup)
    {
        PlayButtonClickSFX();
        if (popup != null)
        {
            popup.SetActive(true);
        }
    }

    public void ClosePopup(GameObject popup)
    {
        PlayButtonClickSFX();
        if (popup != null)
        {
            popup.SetActive(false);
        }
    }
}