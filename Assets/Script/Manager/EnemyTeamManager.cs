using System;
using System.Collections;
using UnityEngine;

public class EnemyTeamManager : MonoBehaviour
{
    public static EnemyTeamManager Instance { get; private set; }

    [Header("Team Setup")]
    // Variabel list musuh dihapus, karena kita akan "minta" langsung ke GameManager
    [SerializeField] private PlayerFighterBase player;

    [Header("Special Attack Thresholds")]
    [Tooltip("Urutkan dari yang terbesar (misal: 0.75, 0.5, 0.25)")]
    [SerializeField] private float[] hpThresholds = { 0.75f, 0.5f, 0.25f };
    private bool[] thresholdFired;

    public int TotalMaxHP { get; private set; }
    public int TotalCurrentHP { get; private set; }

    public static event Action<int, int> OnTotalHpChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private IEnumerator Start()
    {
        // Tunggu 1 frame agar semua musuh selesai mendaftarkan dirinya ke GameManager
        yield return null;

        thresholdFired = new bool[hpThresholds.Length];

        TotalMaxHP = 0;

        // Ambil data musuh langsung dari GameManager tanpa perlu di-drag
        if (GameManager.Instance != null)
        {
            foreach (var enemy in GameManager.Instance.activeEnemies)
            {
                TotalMaxHP += enemy.MaxHP;
            }

            Debug.Log($"Total Enemy Max HP: {TotalMaxHP}");
        }

        TotalCurrentHP = TotalMaxHP;
        OnTotalHpChanged?.Invoke(TotalCurrentHP, TotalMaxHP);
    }

    public void ReportDamage(int damageAmount)
    {
        TotalCurrentHP = Mathf.Max(0, TotalCurrentHP - damageAmount);
        OnTotalHpChanged?.Invoke(TotalCurrentHP, TotalMaxHP);
        Debug.Log($"Total Enemy HP: {TotalCurrentHP}/{TotalMaxHP} (Damage Dilaporkan: {damageAmount})");
        CheckThresholds();
    }

    private void CheckThresholds()
    {
        float ratio = (float)TotalCurrentHP / TotalMaxHP;

        for (int i = 0; i < hpThresholds.Length; i++)
        {
            if (!thresholdFired[i] && ratio <= hpThresholds[i])
            {
                thresholdFired[i] = true;
                StartCoroutine(OrchestrateSpecialAttackPhase());
                break;
            }
        }
    }

    private IEnumerator OrchestrateSpecialAttackPhase()
    {
        // 1. Bekukan input minigame ketik via sinyal global
        CombatFreezeSignal.Begin();

        // 2. Paksa Player berhenti nyerang, mundur, dan simpan serangannya ke buffer
        if (player != null) player.ForceIdleAndBuffer();

        // 3. Paksa semua musuh kembali ke idle
        EnemyFighterBase chosenAttacker = null;

        // Looping langsung dari daftar musuh milik GameManager
        if (GameManager.Instance != null)
        {
            foreach (var enemy in GameManager.Instance.activeEnemies)
            {
                if (!enemy.IsDead)
                {
                    enemy.ForceIdleAndBuffer();

                    // Jadikan musuh pertama yang kita temukan sebagai eksekutor animasinya
                    if (chosenAttacker == null) chosenAttacker = enemy;
                }
            }
        }

        // Beri jeda sedikit agar transisi kembali ke posisinya terlihat natural
        yield return new WaitForSeconds(0.5f);

        // 4. Eksekusi serangan spesial sinematik (slow-mo) oleh musuh terpilih
        if (chosenAttacker != null && player != null)
        {
            yield return chosenAttacker.ExecuteCinematicSpecialAttack(player);
        }

        // 5. Kembalikan permainan, buffer yang tersimpan akan otomatis tereksekusi
        CombatFreezeSignal.End();
    }
}