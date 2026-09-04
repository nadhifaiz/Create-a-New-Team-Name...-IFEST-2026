using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Level Info")]
    public int currentLevelIndex = 1;

    [Header("Game State Flags")]
    public bool isPaused { get; private set; }
    public bool isGameOver { get; private set; }

    [Header("Match Results")]
    public float matchTimer { get; private set; }
    public int finalWPM { get; private set; }
    public float finalAccuracy { get; private set; }

    public List<EnemyFighterBase> activeEnemies = new List<EnemyFighterBase>();

    public event Action<bool> OnPauseToggled;
    public event Action OnLevelComplete;
    public event Action OnGameOver;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        if (!isPaused && !isGameOver)
        {
            matchTimer += Time.deltaTime;
        }
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        OnPauseToggled?.Invoke(isPaused);
    }

    public void CheckWinCondition(EnemyFighterBase deadEnemy)
    {
        if (isGameOver) return;
        activeEnemies.Remove(deadEnemy);
        if (activeEnemies.Count == 0) TriggerLevelComplete();
    }

    private void TriggerLevelComplete()
    {
        if (isGameOver) return;
        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f;

        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveLevelResult(currentLevelIndex, finalWPM, finalAccuracy, matchTimer);
        }

        OnLevelComplete?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }

    public void UpdateTypingStats(int wpm, float accuracy)
    {
        finalWPM = wpm;
        finalAccuracy = accuracy;
    }

    // =============================
    // SCENE MANAGEMENT
    // =============================

    private void ResetStateForNewScene()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameOver = false;
        matchTimer = 0f;
        activeEnemies.Clear();
    }

    public void RestartLevel()
    {
        ResetStateForNewScene();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Dipanggil saat pemain selesai level dan memilih 'Next Level'
    public void LoadNextLevel()
    {
        ResetStateForNewScene();

        // 1. Naikkan level index untuk catatan level selanjutnya
        currentLevelIndex++;

        // 2. Beri tahu MainMenuManager agar langsung membuka panel Level Selection
        MainMenuManager.openLevelSelectOnLoad = true;

        // 3. Muat scene Main Menu
        SceneManager.LoadScene("MainMenu");
    }

    // Dipanggil saat pemain menekan tombol 'Back to Main Menu' biasa
    public void LoadMainMenu(bool openLevelSelection = false)
    {
        ResetStateForNewScene();
        MainMenuManager.openLevelSelectOnLoad = openLevelSelection;
        SceneManager.LoadScene("MainMenu");
    }

    public void StartLevel(int levelIndex, string sceneName)
    {
        ResetStateForNewScene();
        currentLevelIndex = levelIndex;

        // Simpan level terakhir yang dimainkan
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SetLastPlayedLevel(levelIndex);
        }

        SceneManager.LoadScene(sceneName);
    }
}