using System.Collections.Generic;
using UnityEngine;

// 1. Struktur Data untuk satu level
[System.Serializable]
public class LevelStats
{
    public int levelIndex;
    public bool isUnlocked;
    public int bestWPM;
    public float bestAccuracy;
    public float bestTime; // Waktu tercepat penyelesaian
}

// 2. Wrapper (Bungkusan) untuk disimpan ke JSON
[System.Serializable]
public class SaveDataWrapper
{
    public List<LevelStats> levelDataList = new List<LevelStats>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private const string SAVE_KEY = "GameProgressionData";
    private SaveDataWrapper saveData = new SaveDataWrapper();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadData(); // Muat data saat game baru dibuka
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
            saveData = JsonUtility.FromJson<SaveDataWrapper>(json);
        }
        else
        {
            // Jika belum ada save data sama sekali (pemain baru), 
            // buat data kosong dan buka otomatis Level 1 (Index 0 atau 1, tergantung maumu)
            saveData = new SaveDataWrapper();
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
    // FUNGSI UNTUK DIPANGGIL OLEH GAME MANAGER / UI
    // ==========================================

    // Mengecek apakah suatu level boleh dimainkan
    public bool IsLevelUnlocked(int levelIndex)
    {
        LevelStats stats = GetLevelStats(levelIndex);
        return stats != null && stats.isUnlocked;
    }

    // Membuka level baru
    public void UnlockLevel(int levelIndex)
    {
        LevelStats stats = GetOrCreateLevelStats(levelIndex);
        stats.isUnlocked = true;
        SaveToDisk();
    }

    // Mengambil data statistik untuk ditampilkan di UI Menu Level
    public LevelStats GetLevelStats(int levelIndex)
    {
        return saveData.levelDataList.Find(l => l.levelIndex == levelIndex);
    }

    // Menyimpan rekor kemenangan (Dipanggil GameManager saat Level Complete)
    public void SaveLevelResult(int levelIndex, int wpm, float accuracy, float time)
    {
        LevelStats stats = GetOrCreateLevelStats(levelIndex);
        stats.isUnlocked = true; // Pastikan level ini statusnya terbuka

        // Simpan hanya jika skornya lebih baik (atau simpan langsung jika ini pertama kali)
        // Logika WPM tertinggi:
        if (wpm > stats.bestWPM) stats.bestWPM = wpm;

        // Logika Akurasi tertinggi:
        if (accuracy > stats.bestAccuracy) stats.bestAccuracy = accuracy;

        // Logika Waktu tercepat (jika bestTime masih 0, berarti belum pernah main)
        if (stats.bestTime == 0 || time < stats.bestTime) stats.bestTime = time;

        // Otomatis buka level selanjutnya!
        UnlockLevel(levelIndex + 1);

        SaveToDisk();
        Debug.Log($"Data Level {levelIndex} Tersimpan! WPM: {wpm}, Acc: {accuracy}%, Waktu: {time}s");
    }

    // Helper untuk mencari data level. Jika belum ada, buatkan baru.
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
}