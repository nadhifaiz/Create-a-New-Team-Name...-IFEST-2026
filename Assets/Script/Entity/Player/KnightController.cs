using UnityEngine;
using UnityEngine.InputSystem;

public class KnightController : FighterBase
{
    [Header("Player Specifics")]
    [SerializeField] private int maxDamage = 80;
    [SerializeField] private GameObject comboAura;

    // -- CACHE ANIMATION PARRY (Eksklusif Player) --
    private readonly int animParry = Animator.StringToHash("Parry");

    private int comboCount = 0;
    public bool isTypingComboActive = false;
    public bool isParrying = false;

    protected override void Start()
    {
        base.Start(); // Memanggil setup dasar (HP, StartPos) dari FighterBase

        if (comboAura != null) comboAura.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        HandleCombatInput();
    }

    private void HandleCombatInput()
    {
        // Pastikan New Input System terbaca
        if (Keyboard.current == null) return;

        // Tombol Serang (J)
        if (Keyboard.current.jKey.wasPressedThisFrame && !isParrying)
        {
            TriggerAttack();
        }

        // Tombol Tangkis (K)
        if (Keyboard.current.kKey.wasPressedThisFrame && !isAttacking && !isParrying)
        {
            Parry();
        }
    }

    // ==========================================
    // OVERRIDE MEKANIK DAMAGE & PARRY
    // ==========================================

    public void Parry()
    {
        if (isDead) return;
        isParrying = true;
        anim.Play(animParry);
    }

    // Dipanggil lewat Animation Event di akhir frame animasi Parry
    public void ResetActionState()
    {
        isParrying = false;
    }

    protected override void Die()
    {
        base.Die();
        if (comboAura != null) comboAura.SetActive(false);
        Debug.Log("Player Mati!");
    }

    // ==========================================
    // SISTEM KOMBO KETIKAN
    // ==========================================
    public void ApplyTypingComboEffect(bool isCombo)
    {
        isTypingComboActive = isCombo;

        if (isCombo)
        {
            comboCount++;

            if (comboAura != null) comboAura.SetActive(true);

            // Perhitungan Combo sesuai permintaan
            if (comboCount == 1)
            {
                IncreaseDamage(5);
            }
            else if (comboCount > 1)
            {
                IncreaseDamage(1);
            }
        }
        else
        {
            // Salah ngetik = Combo Hangus
            comboCount = 0;
            if (comboAura != null) comboAura.SetActive(false);
            currDamage = baseDamage; // Reset ke damage awal
        }
    }

    private void IncreaseDamage(int amount)
    {
        int temp = currDamage + amount;

        // Memastikan damage mentok di angka maxDamage
        if (temp > maxDamage)
        {
            currDamage = maxDamage;
        }
        else
        {
            currDamage = temp;
        }
    }
}