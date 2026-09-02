using System.Collections;
using UnityEngine;
using System;

public abstract class EnemyFighterBase : FighterBase
{
    [Header("AI Settings")]
    [SerializeField] private float minAttackInterval = 1.5f;
    [SerializeField] private float maxAttackInterval = 3f;
    [Range(0f, 1f)]
    [SerializeField] private float missChance = 0.2f;

    [Header("Special Attack Settings")]
    [SerializeField] private float specialDamageMultiplier = 2f;

    // -- CACHE ANIMATION SPECIAL ATTACK (Eksklusif Enemy) --
    private readonly int animSpecialAttack = Animator.StringToHash("SpecialAttack");
    public event Action OnAttackMissed;

    public int CurrentHP => currHP;
    public int MaxHP => maxHP;

    private bool nextAttackIsMiss = false;

    protected override void Start()
    {
        base.Start();

        StartCoroutine(AIRoutine());

        // Daftarkan musuh ini ke dalam List milik GameManager saat game mulai
        if (GameManager.Instance != null)
        {
            GameManager.Instance.activeEnemies.Add(this);
        }
    }

    private IEnumerator AIRoutine()
    {
        while (!isDead)
        {
            float waitTime = UnityEngine.Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            if (isDead) yield break;
            if (isAttacking) continue;

            float missRoll = UnityEngine.Random.value;
            nextAttackIsMiss = missRoll < missChance;
            Debug.Log($"[{name}] roll miss = {missRoll:F2} (ambang missChance = {missChance}) -> {(nextAttackIsMiss ? "AKAN MELESET" : "AKAN KENA")}");

            TriggerAttack();
        }
    }

    protected override void OnHitImpact()
    {
        if (nextAttackIsMiss)
        {
            Debug.Log($"[{name}] serangan MELESET");
            OnAttackMissed?.Invoke();
            nextAttackIsMiss = false;
            return;
        }

        base.OnHitImpact();
    }

    // ==========================================
    // PENANGANAN DAMAGE & LAPORAN KE TEAM MANAGER
    // ==========================================
    public override void TakeDamage(int damage)
    {
        // Jangan panggil TakeDamage base jika sudah mati
        if (isDead) return;

        int oldHP = currHP;
        base.TakeDamage(damage);

        int damageTaken = oldHP - currHP;

        // Laporkan damage yang masuk ke Sutradara (Team Manager)
        if (damageTaken > 0 && EnemyTeamManager.Instance != null)
        {
            EnemyTeamManager.Instance.ReportDamage(damageTaken);
        }
    }

    // ==========================================
    // EKSEKUSI CINEMATIC QTE (Dipanggil secara eksklusif oleh EnemyTeamManager)
    // ==========================================
    public IEnumerator ExecuteCinematicSpecialAttack(PlayerFighterBase playerTarget)
    {
        isAttacking = true;
        anim.Play(animSpecialAttack);

        // === FASE 1: WIND-UP ===
        // Tunggu sampai tangan musuh terangkat di tengah animasi (misal 0.4 detik)
        // Sesuaikan angka ini dengan momen tepat sebelum impact animasi kamu
        yield return new WaitForSeconds(0.4f);

        // === FASE 2: SLOW MOTION QTE ===
        Time.timeScale = 0.05f; // Melambat sangat drastis
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Sesuaikan fisika Unity agar tidak glitch

        float qteDurationRealtime = 5f; // Waktu yang diberikan: 5 detik asli
        playerTarget.BeginParryChallenge(qteDurationRealtime);

        float timer = 0f;
        bool parried = false;

        // Loop mengecek input parry tanpa terpengaruh efek slow motion (unscaledDeltaTime)
        while (timer < qteDurationRealtime)
        {
            timer += Time.unscaledDeltaTime;

            if (playerTarget.HasFinishedParry)
            {
                parried = playerTarget.IsParrySuccess;
                break; // Jika pemain selesai nekan sebelum 5 detik, langsung keluar dari loop slow-mo!
            }
            yield return null;
        }

        // === FASE 3: KEMBALIKAN WAKTU & IMPACT ===
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Tutup UI Parry
        playerTarget.ResolveParryUI(parried);

        // Beri sisa waktu animasi pukulan (misal 0.2 detik)
        yield return new WaitForSeconds(0.2f);

        if (!parried)
        {
            int specialDamage = Mathf.RoundToInt(currDamage * specialDamageMultiplier);
            playerTarget.TakeSpecialDamage(specialDamage);
        }

        anim.Play(animIdle);
        isAttacking = false;
    }

    protected override void Die()
    {
        base.Die();

        // Beritahu GameManager bahwa musuh ini mati agar dicek sisa musuhnya
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CheckWinCondition(this);
        }
    }
}