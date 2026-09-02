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

    public void LoadNextLevel()
    {
        ResetStateForNewScene();
        currentLevelIndex++;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("Tidak ada scene selanjutnya! Kembali ke Main Menu.");
            LoadMainMenu();
        }
    }

    public void LoadMainMenu()
    {
        ResetStateForNewScene();
        SceneManager.LoadScene(0);
    }
}