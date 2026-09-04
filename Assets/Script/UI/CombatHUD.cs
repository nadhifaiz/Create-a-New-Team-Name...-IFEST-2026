using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private Image[] enemyHPBars;

    [Header("Parry QTE HUD")]
    [SerializeField] private GameObject parryPanel;
    [SerializeField] private TMP_Text parryKeyText;
    [SerializeField] private Image parryProgressBar;

    [Header("Floating Miss Text")]
    [SerializeField] private GameObject missTextPrefab;

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

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].OnAttackMissed += SpawnMissText;
                    enemies[i].OnHpChanged += UpdateEnemyHP;
                }
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

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    enemies[i].OnAttackMissed -= SpawnMissText;
                    enemies[i].OnHpChanged -= UpdateEnemyHP;
                }
            }
        }
    }

    private void Start()
    {
        if (player != null)
        {
            UpdateHP(player.CurrentHP, player.MaxHP);
            UpdateDamage(player.CurrentDamage);
            UpdateCombo(player.CurrentCombo);
            HideParry();
        }

        if (enemies != null)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    UpdateEnemyHP(enemies[i], enemies[i].CurrentHP, enemies[i].MaxHP);
                }
            }
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
        if (comboText != null) comboText.text = $"+ {currentCombo}";
    }

    // --- FUNGSI UPDATE HP MUSUH ---
    private void UpdateEnemyHP(EnemyFighterBase enemy, int currentHp, int maxHp)
    {
        int index = System.Array.IndexOf(enemies, enemy);

        if (index >= 0 && index < enemyHPBars.Length && enemyHPBars[index] != null)
        {
            if (maxHp > 0)
            {
                float progress = Mathf.Clamp01((float)currentHp / maxHp);
                enemyHPBars[index].fillAmount = progress;
            }
        }
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

    private void SpawnMissText(Transform enemy)
    {
        if (missTextPrefab != null && Camera.main != null && enemy != null)
        {
            Vector3 worldPosition = enemy.position + new Vector3(0f, 2f, 0f);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            Instantiate(missTextPrefab, screenPosition, Quaternion.identity, transform);
        }
    }
}