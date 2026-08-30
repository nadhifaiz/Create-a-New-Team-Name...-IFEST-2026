using UnityEngine;
using UnityEngine.InputSystem;

public class ElfController : FighterBase
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
        base.Start();

        if (comboAura != null) comboAura.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        HandleCombatInput();
    }

    // INI LOGIKA DUMMY YAHH!!! (Attack & Parry)
    private void HandleCombatInput()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.jKey.wasPressedThisFrame && !isParrying)
        {
            TriggerAttack();
        }
        if (Keyboard.current.kKey.wasPressedThisFrame && !isAttacking && !isParrying)
        {
            Parry();
        }
    }

    // ==========================================
    // MEKANIK PARRY
    // ==========================================

    public void Parry()
    {
        if (isDead) return;
        isParrying = true;
        anim.Play(animParry);
    }

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
            IncreaseDamage(5);
        }
        else
        {
            comboCount = 0;
            if (comboAura != null) comboAura.SetActive(false);
            currDamage = baseDamage;
        }
    }

    private void IncreaseDamage(int amount)
    {
        int temp = currDamage + amount;

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