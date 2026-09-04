using UnityEngine;
using UnityEngine.InputSystem;
using System;

public abstract class PlayerFighterBase : FighterBase
{
    [Header("Player Specifics")]
    [SerializeField] protected int maxDamage = 80;
    [SerializeField] protected GameObject comboAura;

    [Header("Combo Tuning")]
    [SerializeField] protected int comboFirstBonus = 5;
    [SerializeField] protected int comboRepeatBonus = 1;

    [Header("Parry QTE (respon Special Attack musuh)")]
    [SerializeField] protected int parryRequiredTaps = 8;

    public event Action<int, int> OnHpChanged;
    public event Action<int> OnDamageChanged;
    public event Action<int> OnComboChanged;
    public event Action<string, float> OnParryUpdated;
    public event Action OnParryEnded;

    // Getter untuk nilai awal saat Start
    public int CurrentHP => currHP;
    public int MaxHP => maxHP;
    public int MaxDamage => maxDamage;
    public int CurrentDamage => currDamage;
    public int CurrentCombo => comboCount;

    // -- CACHE ANIMATION (Eksklusif Player) --
    // Parry: TRANSISI 1 = Idle -> Parry (BeginParryChallenge),
    //        TRANSISI 2 = Parry -> Idle (ResolveParryChallenge, pakai animIdle
    //        yang udah ada di FighterBase — gak perlu hash baru buat exit-nya)
    private readonly int animParry = Animator.StringToHash("Parry");

    // Reaksi KHUSUS kena special attack — beda dari animHit biasa (global) di FighterBase
    private readonly int animHitSpecial = Animator.StringToHash("HitSpecial");

    private static readonly Key[] letterKeys =
    {
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J,
        Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T,
        Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z
    };

    private int comboCount = 0;
    public bool isTypingComboActive = false;
    public bool isParrying = false;

    // State QTE parry — di-drive dari luar lewat BeginParryChallenge/ResolveParryChallenge
    private bool isParryChallengeActive = false;
    private bool parryChallengeSuccess = false;
    private int parryTapCount = 0;
    private Key parryTargetKey;

    public Key CurrentParryKey => parryTargetKey;
    public float ParryProgress01 => parryRequiredTaps <= 0 ? 1f : Mathf.Clamp01((float)parryTapCount / parryRequiredTaps);

    protected override void Start()
    {
        base.Start();

        if (comboAura != null) comboAura.SetActive(false);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (isParryChallengeActive)
        {
            HandleParryChallengeInput();
            return;
        }
    }

    private void HandleParryChallengeInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[parryTargetKey].wasPressedThisFrame)
        {
            parryTapCount++;

            // Panggil event update progress bar parry
            OnParryUpdated?.Invoke(parryTargetKey.ToString(), ParryProgress01);

            if (parryTapCount >= parryRequiredTaps) parryChallengeSuccess = true;
        }
    }

    // ==========================================
    // PARRY QTE — dipanggil dari EnemyFighterBase.SpecialAttackFlow()
    // ==========================================

    // TRANSISI 1: Idle normal -> Parry.
    public void BeginParryChallenge(float duration)
    {
        if (isDead) return;

        isParrying = true;
        isParryChallengeActive = true;
        parryChallengeSuccess = false;
        parryTapCount = 0;
        parryTargetKey = letterKeys[UnityEngine.Random.Range(0, letterKeys.Length)];
        Debug.Log($"[{name}] PARRY QTE DIMULAI — spam tombol {parryTargetKey} sebanyak {parryRequiredTaps}x, waktu {duration}s");

        anim.Play(animParry);
        OnParryUpdated?.Invoke(parryTargetKey.ToString(), 0f);
    }

    // TRANSISI 2: Parry -> Idle normal. Dipanggil enemy pas durasi special attack-nya abis.
    public bool ResolveParryChallenge()
    {
        isParryChallengeActive = false;
        isParrying = false;
        anim.Play(animIdle);
        OnParryEnded?.Invoke(); // Picu event untuk tutup UI parry

        return parryChallengeSuccess;
    }

    // ==========================================
    // TERKENA DAMAGE SPECIAL ATTACK (KHUSUS PLAYER)
    // ==========================================
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        OnHpChanged?.Invoke(currHP, maxHP); // Picu event HP berubah
    }
    public void TakeSpecialDamage(int damage)
    {
        if (isDead) return;

        ApplyDamage(damage);

        OnHpChanged?.Invoke(currHP, maxHP); // Picu event HP berubah

        if (!isDead)
        {
            anim.Play(animHitSpecial);
        }
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

            if (comboCount == 1)
            {
                IncreaseDamage(comboFirstBonus);
            }
            else
            {
                IncreaseDamage(comboRepeatBonus);
            }

            Debug.Log($"[{name}] combo x{comboCount} -> damage {currDamage}");
        }
        else
        {
            comboCount = 0;
            if (comboAura != null) comboAura.SetActive(false);
            currDamage = baseDamage;
            Debug.Log($"[{name}] combo hangus -> damage kembali ke {currDamage}");
        }

        OnComboChanged?.Invoke(comboCount); // Picu event Combo
        OnDamageChanged?.Invoke(currDamage); // Picu event Damage
    }

    private void IncreaseDamage(int amount)
    {
        int temp = currDamage + amount;
        currDamage = temp > maxDamage ? maxDamage : temp;
    }
}