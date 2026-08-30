using System.Collections;
using UnityEngine;

// ==========================================
// SEMUA KARAKTER ENEMY (Goblin, Dragon, ShadowMaster) TURUN DARI SINI
// Isinya: AI nyerang interval random + chance gagal.
// Cara serang (melee/projectile) DIATUR DI FighterBase / leaf class,
// jadi kelas ini gak perlu tau bedanya sama sekali.
// ==========================================
public abstract class EnemyFighterBase : FighterBase
{
    [Header("AI Settings")]
    [SerializeField] private float minAttackInterval = 1.5f;
    [SerializeField] private float maxAttackInterval = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float missChance = 0.2f;

    [Header("Special Attack (Phase Trigger)")]
    // Urutin dari besar ke kecil. Tiap angka = persentase HP yang begitu
    // ditembus (turun ke bawahnya) langsung mancing special attack sekali.
    [SerializeField] private float[] specialAttackHpThresholds = { 0.75f, 0.5f, 0.25f };
    [SerializeField] private float specialAttackDuration = 1.5f; // durasi animasi = durasi window QTE player
    [SerializeField] private float specialDamageMultiplier = 2f;

    // -- CACHE ANIMATION SPECIAL ATTACK (Eksklusif Enemy) --
    private readonly int animSpecialAttack = Animator.StringToHash("SpecialAttack");

    // Flag internal buat nandain apakah serangan berikutnya meleset
    private bool nextAttackIsMiss = false;

    // Nandain threshold mana aja yang udah pernah kepake, biar gak nembak berkali-kali
    private bool[] specialAttackFired;

    protected override void Start()
    {
        base.Start();
        specialAttackFired = new bool[specialAttackHpThresholds.Length];
        StartCoroutine(AIRoutine());
    }

    private IEnumerator AIRoutine()
    {
        while (!isDead)
        {
            // Tunggu interval random sebelum nyoba nyerang lagi
            float waitTime = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            if (isDead) yield break;

            // Kalau masih di tengah AttackFlow, skip dulu (gak buffer serangan
            // kayak player combo — AI nyerang satu-satu aja)
            if (isAttacking) continue;

            // Tentukan apakah serangan kali ini bakal gagal/meleset
            nextAttackIsMiss = Random.value < missChance;

            TriggerAttack();
        }
    }

    // Override impact: kalau lagi "miss", ApplyImpactEffect (damage/projectile)
    // di-skip, tapi animasi & flow attack tetap jalan normal
    protected override void OnHitImpact()
    {
        if (nextAttackIsMiss)
        {
            Debug.Log($"[{name}] serangan MELESET");
            nextAttackIsMiss = false;
            return;
        }

        base.OnHitImpact();
    }

    // ==========================================
    // SPECIAL ATTACK — DIPICU OTOMATIS SAAT HP NEMBUS FASE TERTENTU
    // ==========================================
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (isDead) return;

        CheckSpecialAttackThreshold();
    }

    private void CheckSpecialAttackThreshold()
    {
        // Kalau lagi sibuk (dash/attack/special lain), tunda dulu —
        // TakeDamage berikutnya bakal cek ulang.
        if (isAttacking) return;

        float hpRatio = (float)currHP / maxHP;

        for (int i = 0; i < specialAttackHpThresholds.Length; i++)
        {
            if (specialAttackFired[i]) continue;

            if (hpRatio <= specialAttackHpThresholds[i])
            {
                specialAttackFired[i] = true;
                Debug.Log($"[{name}] HP nembus {specialAttackHpThresholds[i] * 100}% -> SPECIAL ATTACK!");
                StartCoroutine(SpecialAttackFlow());
                break; // Satu special attack per hit, biar gak numpuk kalau damage-nya gede
            }
        }
    }

    private IEnumerator SpecialAttackFlow()
    {
        isAttacking = true; // Blok AIRoutine & TriggerAttack biasa selama special attack jalan
        anim.Play(animSpecialAttack);

        // Kasih tau player buat mulai QTE parry, BARENGAN sama animasi enemy jalan
        PlayerFighterBase playerTarget = opponent != null ? opponent.GetComponent<PlayerFighterBase>() : null;
        playerTarget?.BeginParryChallenge(specialAttackDuration);

        yield return new WaitForSeconds(specialAttackDuration);

        bool parried = playerTarget != null && playerTarget.ResolveParryChallenge();
        Debug.Log($"[{name}] window parry abis -> hasil: {(parried ? "PLAYER BERHASIL PARRY" : "PLAYER GAGAL PARRY")}");

        if (!parried && playerTarget != null)
        {
            int specialDamage = Mathf.RoundToInt(currDamage * specialDamageMultiplier);
            playerTarget.TakeDamage(specialDamage);
        }

        anim.Play(animIdle);
        isAttacking = false;
    }
}