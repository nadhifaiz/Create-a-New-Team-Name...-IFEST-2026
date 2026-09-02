using System;
using System.Collections.Generic; // Wajib ditambahkan untuk List
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State Flags")]
    public bool isPaused { get; private set; }
    public bool isGameOver { get; private set; }

    // Ubah Array menjadi List agar dinamis
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
    }

    public void TogglePause()
    {
        if (isGameOver) return;

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        OnPauseToggled?.Invoke(isPaused);
    }

    // Fungsi baru untuk mengecek apakah semua musuh sudah mati
    public void CheckWinCondition(EnemyFighterBase deadEnemy)
    {
        if (isGameOver) return;

        // Hapus musuh yang baru saja mati dari daftar
        activeEnemies.Remove(deadEnemy);

        // Jika daftar kosong (semua musuh habis), picu Level Complete
        if (activeEnemies.Count == 0)
        {
            TriggerLevelComplete();
        }
    }

    private void TriggerLevelComplete()
    {
        if (isGameOver) return;

        Debug.Log("Semua musuh kalah! LEVEL COMPLETE!");
        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f;
        OnLevelComplete?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        Debug.Log("Player mati! GAME OVER!");
        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f;
        OnGameOver?.Invoke();
    }
}