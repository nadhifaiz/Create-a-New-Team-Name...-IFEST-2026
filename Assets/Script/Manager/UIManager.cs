using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject combatHUD;
    [SerializeField] private GameObject settingsPanel;

    [Header("GameOver Result UI")]
    [SerializeField] private TextMeshProUGUI gameOverWPMText;
    [SerializeField] private TextMeshProUGUI gameOverAccuracyText;
    [SerializeField] private Image gameOverScore;

    [Header("Win Result UI")]
    [SerializeField] private TextMeshProUGUI finalWPMText;
    [SerializeField] private TextMeshProUGUI finalAccuracyText;
    [SerializeField] private Image scoreProgressBar;

    [Header("Match Timer UI")]
    [SerializeField] private TextMeshProUGUI matchTimerText;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (combatHUD != null) combatHUD.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseToggled += HandlePause;
            GameManager.Instance.OnLevelComplete += HandleLevelComplete;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.isPaused && !GameManager.Instance.isGameOver)
        {
            if (matchTimerText != null)
            {
                TimeSpan time = TimeSpan.FromSeconds(GameManager.Instance.matchTimer);
                matchTimerText.text = time.ToString(@"mm\:ss");
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseToggled -= HandlePause;
            GameManager.Instance.OnLevelComplete -= HandleLevelComplete;
            GameManager.Instance.OnGameOver -= HandleGameOver;
        }
    }

    private void HandlePause(bool isPaused)
    {
        PlayButtonClickSFX();
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    private void HandleLevelComplete()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
        if (combatHUD != null) combatHUD.SetActive(false);

        UpdateResultUI();
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (combatHUD != null) combatHUD.SetActive(false);

        UpdateResultUI();
    }

    // Fungsi ini sekarang menyalurkan data ke panel Win dan Game Over sekaligus
    private void UpdateResultUI()
    {
        if (GameManager.Instance == null) return;

        // --- Update UI Menang ---
        if (finalWPMText != null)
            finalWPMText.text = $"WPM: {GameManager.Instance.finalWPM} WPM";

        if (finalAccuracyText != null)
            finalAccuracyText.text = $"Accuracy: {GameManager.Instance.finalAccuracy:F1}%";

        if (scoreProgressBar != null)
            scoreProgressBar.fillAmount = GameManager.Instance.finalAccuracy / 100f;

        // --- Update UI Kalah ---
        if (gameOverWPMText != null)
            gameOverWPMText.text = $"WPM: {GameManager.Instance.finalWPM} WPM";

        if (gameOverAccuracyText != null)
            gameOverAccuracyText.text = $"Accuracy: {GameManager.Instance.finalAccuracy:F1}%";

        if (gameOverScore != null)
            gameOverScore.fillAmount = GameManager.Instance.finalAccuracy / 100f;
    }

    // =============================
    // FUNGSI UNTUK TOMBOL UI (Button OnClick)
    // =============================

    public void Button_ResumeGame()
    {
        PlayButtonClickSFX();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    public void Button_RestartLevel()
    {
        PlayButtonClickSFX();
        if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
    }

    public void Button_NextLevel()
    {
        PlayButtonClickSFX();
        if (GameManager.Instance != null) GameManager.Instance.LoadNextLevel();
    }

    public void Button_MainMenu()
    {
        PlayButtonClickSFX();
        if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
    }

    public void Button_OpenSettings()
    {
        PlayButtonClickSFX();
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void Button_CloseSettings()
    {
        PlayButtonClickSFX();
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void PlayButtonClickSFX()
    {
        if (AudioSystem.Instance != null)
        {
            AudioSystem.Instance.PlayAudio(AudioSystem.Instance.sfx_HitPlay);
        }
    }
}