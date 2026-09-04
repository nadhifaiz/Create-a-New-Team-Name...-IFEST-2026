using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CombatHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerFighterBase player;
    [SerializeField] private EnemyFighterBase[] enemies;

    [Header("Player Stats")]
    [SerializeField] private Image progressHP;
    [SerializeField] private Image progressDamage;
    [SerializeField] private TMP_Text comboText;

    [Header("Enemy Stats")]
    [SerializeField] private Image totalEnemyHPBar;

    [Header("Parry QTE HUD")]
    [SerializeField] private GameObject parryPanel;
    [SerializeField] private TMP_Text parryKeyText;
    [SerializeField] private Image parryProgressBar;

    [Header("Floating Miss Text")]
    [SerializeField] private GameObject missTextPrefab;
    [SerializeField] private RectTransform missTextSpawnPoint;

    private void Awake()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<PlayerFighterBase>();
        }

        if (enemies == null || enemies.Length == 0 || enemies[0] == null)
        {
            enemies = FindObjectsByType<EnemyFighterBase>();
        }

        HideParry();
        if (comboText != null)
        {
            comboText.text = "+0";
        }
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.OnHpChanged += UpdateHP;
            player.OnDamageChanged += UpdateDamage;
            player.OnComboChanged += UpdateCombo;
            player.OnParryUpdated += ShowAndRefreshParry;
            player.OnParryEnded += HideParry;
        }

        EnemyTeamManager.OnTotalHpChanged += UpdateTotalEnemyHP;

        if (enemies != null)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null) enemy.OnAttackMissed += SpawnMissText;
            }
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.OnHpChanged -= UpdateHP;
            player.OnDamageChanged -= UpdateDamage;
            player.OnComboChanged -= UpdateCombo;
            player.OnParryUpdated -= ShowAndRefreshParry;
            player.OnParryEnded -= HideParry;
        }

        // Unsubscribe langsung ke class
        EnemyTeamManager.OnTotalHpChanged -= UpdateTotalEnemyHP;

        if (enemies != null)
        {
            foreach (var enemy in enemies)
            {
                if (enemy != null) enemy.OnAttackMissed -= SpawnMissText;
            }
        }
    }

    private IEnumerator Start()
    {
        yield return null; // Tunggu 1 frame

        if (player != null)
        {
            UpdateHP(player.CurrentHP, player.MaxHP);
            UpdateDamage(player.CurrentDamage);
            UpdateCombo(player.CurrentCombo);
            HideParry();
        }

        // Tarik nilai awal musuh jika sudah selesai kalkulasi
        if (EnemyTeamManager.Instance != null && EnemyTeamManager.Instance.TotalMaxHP > 0)
        {
            UpdateTotalEnemyHP(EnemyTeamManager.Instance.TotalCurrentHP, EnemyTeamManager.Instance.TotalMaxHP);
        }
    }

    private void UpdateHP(int currentHp, int maxHp)
    {
        if (progressHP != null && maxHp > 0)
        {
            float progress = Mathf.Clamp01((float)currentHp / maxHp);
            progressHP.fillAmount = progress;
        }
    }

    private void UpdateDamage(int currentDamage)
    {
        if (progressDamage != null && player != null)
        {
            float maxDmg = player.MaxDamage;
            float progress = Mathf.Clamp01((float)currentDamage / maxDmg);

            progressDamage.fillAmount = progress;
        }
    }

    private void UpdateCombo(int currentCombo)
    {
        if (comboText != null) comboText.text = $"+{currentCombo}";
    }

    // --- FUNGSI UPDATE HP TOTAL MUSUH ---
    private void UpdateTotalEnemyHP(int currentTotal, int maxTotal)
    {
        if (totalEnemyHPBar != null && maxTotal > 0)
        {
            float progress = Mathf.Clamp01((float)currentTotal / maxTotal);
            totalEnemyHPBar.fillAmount = progress;
        }
        Debug.Log($"Total Enemy HP Updated: {currentTotal}/{maxTotal}");
    }

    private void ShowAndRefreshParry(string targetKey, float progress)
    {
        if (parryPanel != null && !parryPanel.activeSelf) parryPanel.SetActive(true);

        if (parryKeyText != null) parryKeyText.text = targetKey;
        if (parryProgressBar != null) parryProgressBar.fillAmount = progress;
    }

    private void HideParry()
    {
        if (parryPanel != null) parryPanel.SetActive(false);
    }

    private void SpawnMissText()
    {
        if (missTextPrefab != null && missTextSpawnPoint != null)
        {
            Instantiate(missTextPrefab, missTextSpawnPoint.position, Quaternion.identity, transform);
        }
    }
}