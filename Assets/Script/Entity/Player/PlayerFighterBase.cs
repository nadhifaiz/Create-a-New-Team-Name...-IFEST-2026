using UnityEngine;
using UnityEngine.InputSystem;

// ==========================================
// SEMUA KARAKTER PLAYER (Knight, Flail, Elf) TURUN DARI SINI
// Isinya: input keyboard, sistem combo ketik, parry.
// Cara serang (melee/projectile) DIATUR DI FighterBase,
// jadi kelas ini gak perlu tau bedanya sama sekali.
// ==========================================
public abstract class PlayerFighterBase : FighterBase
{
    [Header("Player Specifics")]
    [SerializeField] protected int maxDamage = 80;
    [SerializeField] protected GameObject comboAura;

    [Header("Combo Tuning")]
    // Beda-in nilai ini per karakter lewat Inspector.
    // Knight/Flail: firstBonus 5, repeatBonus 1 (progresif).
    // Elf: firstBonus 5, repeatBonus 5 (flat tiap combo).
    [SerializeField] protected int comboFirstBonus = 5;
    [SerializeField] protected int comboRepeatBonus = 1;

    [Header("Parry QTE (respon Special Attack musuh)")]
    // Berapa kali huruf harus di-spam sebelum window QTE abis
    [SerializeField] protected int parryRequiredTaps = 8;

    // -- CACHE ANIMATION PARRY (Eksklusif Player) --
    private readonly int animParry = Animator.StringToHash("Parry");

    // Huruf A-Z buat dipilih random tiap QTE muncul
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

    public Key CurrentParryKey => parryTargetKey; // buat ditampilkan di UI ("Spam tombol X!")
    public float ParryProgress01 => parryRequiredTaps <= 0 ? 1f : Mathf.Clamp01((float)parryTapCount / parryRequiredTaps);

    protected override void Start()
    {
        base.Start(); // Memanggil setup dasar (HP, StartPos) dari FighterBase

        if (comboAura != null) comboAura.SetActive(false);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        if (isParryChallengeActive)
        {
            // Selama QTE aktif, kunci input serang/parry manual —
            // fokus cuma nampung spam tombol huruf
            HandleParryChallengeInput();
            return;
        }

        HandleCombatInput();
    }

    private void HandleParryChallengeInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current[parryTargetKey].wasPressedThisFrame)
        {
            parryTapCount++;
            Debug.Log($"[{name}] tap {parryTargetKey} -> {parryTapCount}/{parryRequiredTaps}");

            if (parryTapCount >= parryRequiredTaps)
            {
                parryChallengeSuccess = true;
            }
        }
    }

    protected virtual void HandleCombatInput()
    {
        // Pastikan New Input System terbaca
        if (Keyboard.current == null) return;

        // Tombol Serang (J)
        if (Keyboard.current.jKey.wasPressedThisFrame && !isParrying)
        {
            TriggerAttack();
        }

        // Parry manual (K) DIHAPUS — satu-satunya cara parry sekarang lewat
        // QTE yang dipicu enemy pas special attack (lihat BeginParryChallenge).
    }

    // ==========================================
    // PARRY QTE — DIPANGGIL DARI EnemyFighterBase.SpecialAttackFlow()
    // ==========================================

    // Dipanggil pas musuh mulai mainin animasi special attack.
    // Karakter ganti state jadi parry (anim: berdiri -> tancep pedang -> hold idle,
    // diatur di Animator Controller sendiri lewat transition/exit time)
    // dan mulai nampung spam tombol huruf random sampai durasi ini abis.
    public void BeginParryChallenge(float duration)
    {
        if (isDead) return;

        isParrying = true;
        isParryChallengeActive = true;
        parryChallengeSuccess = false;
        parryTapCount = 0;
        parryTargetKey = letterKeys[Random.Range(0, letterKeys.Length)];
        Debug.Log($"[{name}] PARRY QTE DIMULAI — spam tombol {parryTargetKey} sebanyak {parryRequiredTaps}x, waktu {duration}s");

        anim.Play(animParry);
    }

    // Dipanggil enemy pas durasi special attack-nya abis.
    // Balikin true kalau player berhasil ngumpulin tap sebelum waktu habis.
    public bool ResolveParryChallenge()
    {
        isParryChallengeActive = false;
        isParrying = false;
        anim.Play(animIdle);

        return parryChallengeSuccess;
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
            // Salah ngetik = Combo Hangus
            comboCount = 0;
            if (comboAura != null) comboAura.SetActive(false);
            currDamage = baseDamage; // Reset ke damage awal
            Debug.Log($"[{name}] combo hangus -> damage kembali ke {currDamage}");
        }
    }

    private void IncreaseDamage(int amount)
    {
        int temp = currDamage + amount;

        // Memastikan damage mentok di angka maxDamage
        currDamage = temp > maxDamage ? maxDamage : temp;
    }
}