using UnityEngine;
using TMPro; // Wajib untuk TextMeshProUGUI
using UnityEngine.UI; // Wajib untuk Image
using System; // Wajib untuk format TimeSpan timer

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject combatHUD;
    [SerializeField] private GameObject settingsPanel;

    [Header("Match Result UI")]
    [SerializeField] private TextMeshProUGUI finalWPMText;
    [SerializeField] private TextMeshProUGUI finalAccuracyText;
    [SerializeField] private Image scoreProgressBar;

    [Header("Match Timer UI")]
    [SerializeField] private TextMeshProUGUI matchTimerText;

    private void Start()
    {
        // Pastikan semua panel tertutup di awal game
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (combatHUD != null) combatHUD.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Langganan event ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseToggled += HandlePause;
            GameManager.Instance.OnLevelComplete += HandleLevelComplete;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void Update()
    {
        // Update Timer secara real-time di layar saat game sedang berjalan
        if (GameManager.Instance != null && !GameManager.Instance.isPaused && !GameManager.Instance.isGameOver)
        {
            if (matchTimerText != null)
            {
                // Ubah angka detik (float) menjadi format menit:detik (misal 02:30)
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

    // =============================
    // EVENT HANDLERS (DIPANGGIL OLEH GAME MANAGER)
    // =============================

    private void HandlePause(bool isPaused)
    {
        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    private void HandleLevelComplete()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
        if (combatHUD != null) combatHUD.SetActive(false); // Sembunyikan HUD saat result muncul

        UpdateResultUI();
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (combatHUD != null) combatHUD.SetActive(false);

        UpdateResultUI();
    }

    // Fungsi untuk menarik data akhir dari GameManager ke Teks UI
    private void UpdateResultUI()
    {
        if (GameManager.Instance == null) return;

        if (finalWPMText != null)
            finalWPMText.text = $"{GameManager.Instance.finalWPM} WPM";

        if (finalAccuracyText != null)
            finalAccuracyText.text = $"{GameManager.Instance.finalAccuracy:F1}%";

        if (scoreProgressBar != null)
        {
            // Asumsi bar penuh jika akurasi 100% (fillAmount dari 0 sampai 1)
            scoreProgressBar.fillAmount = GameManager.Instance.finalAccuracy / 100f;
        }
    }

    // =============================
    // FUNGSI UNTUK TOMBOL UI (Button OnClick)
    // =============================

    public void Button_ResumeGame()
    {
        if (GameManager.Instance != null) GameManager.Instance.TogglePause();
    }

    public void Button_RestartLevel()
    {
        if (GameManager.Instance != null) GameManager.Instance.RestartLevel();
    }

    public void Button_NextLevel()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadNextLevel();
    }

    public void Button_MainMenu()
    {
        if (GameManager.Instance != null) GameManager.Instance.LoadMainMenu();
    }

    public void Button_OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void Button_CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
}