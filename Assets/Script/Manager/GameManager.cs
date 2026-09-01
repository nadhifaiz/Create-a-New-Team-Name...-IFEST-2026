using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // Singleton Instance
    public static GameManager Instance { get; private set; }

    [Header("Game State Flags")]
    public bool isPaused { get; private set; }
    public bool isGameOver { get; private set; } // Menandakan game sudah berakhir (menang/kalah)

    // Events untuk mendrive UIManager
    public event Action<bool> OnPauseToggled;
    public event Action OnLevelComplete;
    public event Action OnGameOver;

    private void Awake()
    {
        // Setup Singleton & DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Jangan hancurkan saat pindah scene
        }
        else
        {
            Destroy(gameObject); // Cegah ada GameManager ganda
        }
    }

    private void Update()
    {
        // Contoh menggunakan keyboard standar (ESC) untuk pause.
        // Jika pakai New Input System untuk UI, bisa disesuaikan.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    // =============================
    // LOGIKA STATE GAME
    // =============================

    public void TogglePause()
    {
        // Jangan izinkan pause/unpause jika game sudah menang/kalah
        if (isGameOver) return;

        isPaused = !isPaused;

        // Kita tetap set timeScale ke 0 agar AI musuh, fisika, dan partikel berhenti
        Time.timeScale = isPaused ? 0f : 1f;

        // Beritahu UI (UIManager) untuk memunculkan/menyembunyikan panel
        OnPauseToggled?.Invoke(isPaused);
    }

    public void TriggerLevelComplete()
    {
        if (isGameOver) return;

        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f; // Hentikan dunia
        OnLevelComplete?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        isPaused = true;
        Time.timeScale = 0f; // Hentikan dunia
        OnGameOver?.Invoke();
    }
}