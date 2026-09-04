using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelStats
{
    public int levelIndex;
    public bool isUnlocked;
    public int bestWPM;
    public float bestAccuracy;
    public float bestTime;
}

[System.Serializable]
public class GameData
{
    public float bgmVolume = 0.5f;
    public float sfxVolume = 0.5f;
    public bool hasCompletedTutorial = false;
    public int lastPlayedLevelIndex = 1;
    public List<LevelStats> levelDataList = new List<LevelStats>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private const string SAVE_KEY = "GameProgressionData";
    private GameData saveData = new GameData();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // LOGIKA BACA & TULIS DATA
    // ==========================================
    private void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            saveData = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            saveData = new GameData();
            UnlockLevel(1);
        }
    }

    private void SaveToDisk()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    // ==========================================
    // PENGATURAN VOLUME
    // ==========================================
    public float GetBGMVolume() => saveData.bgmVolume;
    public float GetSFXVolume() => saveData.sfxVolume;

    public void SetBGMVolume(float val)
    {
        saveData.bgmVolume = val;
        SaveToDisk();
    }

    public void SetSFXVolume(float val)
    {
        saveData.sfxVolume = val;
        SaveToDisk();
    }

    // ==========================================
    // STATUS TUTORIAL
    // ==========================================
    public bool HasCompletedTutorial() => saveData.hasCompletedTutorial;

    public void CompleteTutorial()
    {
        saveData.hasCompletedTutorial = true;
        SaveToDisk();
    }

    // ==========================================
    // STATISTIK LEVEL
    // ==========================================
    public bool IsLevelUnlocked(int levelIndex)
    {
        LevelStats stats = GetLevelStats(levelIndex);
        return stats != null && stats.isUnlocked;
    }

    public void UnlockLevel(int levelIndex)
    {
        LevelStats stats = GetOrCreateLevelStats(levelIndex);
        stats.isUnlocked = true;
        SaveToDisk();
    }

    public LevelStats GetLevelStats(int levelIndex)
    {
        return saveData.levelDataList.Find(l => l.levelIndex == levelIndex);
    }

    public void SaveLevelResult(int levelIndex, int wpm, float accuracy, float time)
    {
        LevelStats stats = GetOrCreateLevelStats(levelIndex);
        stats.isUnlocked = true;

        if (wpm > stats.bestWPM) stats.bestWPM = wpm;
        if (accuracy > stats.bestAccuracy) stats.bestAccuracy = accuracy;
        if (stats.bestTime == 0 || time < stats.bestTime) stats.bestTime = time;
        UnlockLevel(levelIndex + 1);

        SaveToDisk();
    }

    private LevelStats GetOrCreateLevelStats(int levelIndex)
    {
        LevelStats stats = GetLevelStats(levelIndex);
        if (stats == null)
        {
            stats = new LevelStats { levelIndex = levelIndex, isUnlocked = false };
            saveData.levelDataList.Add(stats);
        }
        return stats;
    }

    // ==========================================
    // LEVEL TERAKHIR YANG DIMAINKAN
    // ==========================================
    public int GetLastPlayedLevel() => saveData.lastPlayedLevelIndex;

    public void SetLastPlayedLevel(int levelIndex)
    {
        saveData.lastPlayedLevelIndex = levelIndex;
        SaveToDisk();
    }

    // ==========================================
    // LOGIKA RESET / HAPUS DATA
    // ==========================================
    [ContextMenu("Reset All Data")] // Bisa diklik kanan langsung dari Inspector buat testing!
    public void DeleteSaveData()
    {
        // 1. Hapus key data dari PlayerPrefs
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerPrefs.Save();
        }

        // 2. Kembalikan data in-memory ke nilai awal/default
        saveData = new GameData();
        UnlockLevel(1); // Level 1 wajib terbuka dari awal

        Debug.Log("Seluruh data save progress berhasil dihapus & di-reset!");
    }
}