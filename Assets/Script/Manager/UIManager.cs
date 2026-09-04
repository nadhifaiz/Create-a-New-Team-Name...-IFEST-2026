using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Start()
    {
        // Pastikan semua panel tertutup di awal game
        if (pausePanel != null) pausePanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Langganan event ke GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPauseToggled += HandlePause;
            GameManager.Instance.OnLevelComplete += HandleLevelComplete;
            GameManager.Instance.OnGameOver += HandleGameOver;
        }
    }

    private void OnDestroy()
    {
        // SANGAT PENTING: Karena GameManager itu abadi (DontDestroyOnLoad), 
        // sedangkan UIManager hancur saat ganti scene, kita WAJIB unsubscribe event di sini.
        // Jika tidak, GameManager akan error mencoba memanggil UI dari scene yang sudah mati.
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
    }

    private void HandleGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    // =============================
    // FUNGSI UNTUK TOMBOL UI (Bisa dipasang di Button OnClick)
    // =============================

    public void Button_ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TogglePause();
        }
    }

    // Nanti untuk scene management (Restart / Next Level), 
    // script pemindahan scenenya bisa dipanggil dari tombol di sini.
}