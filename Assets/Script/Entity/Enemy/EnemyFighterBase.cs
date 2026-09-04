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

    private void OnDestroy()
    {
        // Hapus musuh ini dari List milik GameManager saat musuh dihancurkan
        if (GameManager.Instance != null)
        {
            GameManager.Instance.activeEnemies.Remove(this);
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

            TriggerAttack();
        }
    }

    protected override void OnHitImpact()
    {
        if (nextAttackIsMiss)
        {
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
        Debug.Log($"[{name}] menerima damage {damageTaken}, HP sekarang {currHP}/{maxHP}");

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
        PlayAnim(animSpecialAttack); // Pakai PlayAnim agar ada crossfade

        // Wind-up awal sebelum QTE muncul
        yield return new WaitForSeconds(0.35f);

        // === FASE SLOW MOTION ===
        // Buat animasi musuh tetap bergerak dengan updateMode unscaled agar tidak macet
        if (anim != null) anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        float qteDurationRealtime = 5f;
        playerTarget.BeginParryChallenge(qteDurationRealtime);

        float timer = 0f;
        bool parried = false;

        while (timer < qteDurationRealtime)
        {
            timer += Time.unscaledDeltaTime;

            if (playerTarget.HasFinishedParry)
            {
                parried = playerTarget.IsParrySuccess;
                break;
            }
            yield return null;
        }

        // === KEMBALIKAN WAKTU & RESOLVE ===
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (anim != null) anim.updateMode = AnimatorUpdateMode.Normal;

        playerTarget.ResolveParryUI(parried);

        // Beri waktu animasi pukulan/ayunan mendarat
        yield return new WaitForSeconds(0.25f);

        if (!parried)
        {
            int specialDamage = Mathf.RoundToInt(currDamage * specialDamageMultiplier);
            playerTarget.TakeDamage(specialDamage);
        }

        PlayAnim(animIdle);
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