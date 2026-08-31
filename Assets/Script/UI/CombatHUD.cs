using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerFighterBase player;
    [SerializeField] private EnemyFighterBase[] enemies; // Tambahkan referensi musuh

    [Header("Player Stats")]
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image progressHP;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private Image progressDamage;
    [SerializeField] private TMP_Text comboText;

    [Header("Parry QTE HUD")]
    [SerializeField] private GameObject parryPanel;
    [SerializeField] private TMP_Text parryKeyText;
    [SerializeField] private Image parryProgressBar;

    [Header("Floating Miss Text")]
    [SerializeField] private GameObject missTextPrefab;

    private void OnEnable()
    {
        // Langganan event Player
        if (player != null)
        {
            player.OnHpChanged += UpdateHP;
            player.OnDamageChanged += UpdateDamage;
            player.OnComboChanged += UpdateCombo;
            player.OnParryUpdated += ShowAndRefreshParry;
            player.OnParryEnded += HideParry;
        }

        // Langganan event Musuh
        if (enemies != null)
        {
            foreach (EnemyFighterBase enemy in enemies)
            {
                enemy.OnAttackMissed += SpawnMissText; // Daftarkan fungsi Miss di sini
            }
        }
    }

    private void OnDisable()
    {
        // Berhenti langganan event Player
        if (player != null)
        {
            player.OnHpChanged -= UpdateHP;
            player.OnDamageChanged -= UpdateDamage;
            player.OnComboChanged -= UpdateCombo;
            player.OnParryUpdated -= ShowAndRefreshParry;
            player.OnParryEnded -= HideParry;
        }

        // Berhenti langganan event Musuh
        if (enemies != null)
        {
            foreach (EnemyFighterBase enemy in enemies)
            {
                enemy.OnAttackMissed -= SpawnMissText; // Daftarkan fungsi Miss di sini
            }
        }
    }

    private void Start()
    {
        // Inisialisasi tampilan HUD pertama kali game berjalan
        if (player != null)
        {
            UpdateHP(player.CurrentHP, player.MaxHP);
            UpdateDamage(player.CurrentDamage);
            UpdateCombo(player.CurrentCombo);
            HideParry();
        }
    }

    // --- EVENT HANDLERS ---

    private void UpdateHP(int currentHp, int maxHp)
    {
        hpText.text = $"HP: {currentHp} / {maxHp}";
        float progress = Mathf.Clamp01((float)currentHp / maxHp);
        progressHP.fillAmount = progress;
    }

    private void UpdateDamage(int currentDamage)
    {
        damageText.text = $"DMG: {currentDamage}";
        progressHP.fillAmount = Mathf.Clamp01((float)currentDamage / 80);
    }

    private void UpdateCombo(int currentCombo)
    {
        comboText.text = $"+ {currentCombo}";
    }

    private void ShowAndRefreshParry(string targetKey, float progress)
    {
        if (!parryPanel.activeSelf) parryPanel.SetActive(true);

        parryKeyText.text = targetKey;
        parryProgressBar.fillAmount = progress;
    }

    private void HideParry()
    {
        parryPanel.SetActive(false);
    }

    // --- FUNGSI UNTUK ENEMY MISS (TETAP SAMA) ---
    private void SpawnMissText(Transform enemy)
    {
        if (missTextPrefab != null && Camera.main != null)
        {
            // 1. Ambil posisi musuh di dunia 3D, lalu naikkan 2 unit ke atas kepala
            Vector3 worldPosition = enemy.position + new Vector3(0f, 2f, 0f);

            // 2. Konversi posisi 3D tersebut menjadi posisi 2D di layar (piksel)
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            // 3. Spawn prefab di koordinat layar, jadikan Canvas sebagai parent
            // 'transform' merujuk pada objek tempat script CombatHUD ini menempel
            Instantiate(missTextPrefab, screenPosition, Quaternion.identity, transform);
        }
    }
}