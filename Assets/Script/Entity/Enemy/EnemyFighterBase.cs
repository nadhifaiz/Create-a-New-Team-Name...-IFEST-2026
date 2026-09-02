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

    [Header("Special Attack (Phase Trigger)")]
    [SerializeField] private float[] specialAttackHpThresholds = { 0.75f, 0.5f, 0.25f };
    [SerializeField] private float specialAttackDuration = 1.5f;
    [SerializeField] private float specialDamageMultiplier = 2f;

    // -- CACHE ANIMATION SPECIAL ATTACK (Eksklusif Enemy) --
    private readonly int animSpecialAttack = Animator.StringToHash("SpecialAttack");
    public event Action OnAttackMissed;
    public event Action<EnemyFighterBase, int, int> OnHpChanged;

    public int CurrentHP => currHP;
    public int MaxHP => maxHP;

    private bool nextAttackIsMiss = false;
    private bool[] specialAttackFired;

    protected override void Start()
    {
        base.Start();
        specialAttackFired = new bool[specialAttackHpThresholds.Length];
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
    // SPECIAL ATTACK — DIPICU OTOMATIS SAAT HP NEMBUS FASE TERTENTU
    // ==========================================
    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (isDead) return;
        OnHpChanged?.Invoke(this, currHP, maxHP);

        CheckSpecialAttackThreshold();
    }

    private void CheckSpecialAttackThreshold()
    {
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
                break;
            }
        }
    }

    private IEnumerator SpecialAttackFlow()
    {
        isAttacking = true; // Blok AIRoutine & TriggerAttack biasa selama special attack jalan
        CombatFreezeSignal.Begin(); // Semua enemy lain & minigame ketik ikut berhenti

        Transform target = AcquireTarget();
        anim.Play(animSpecialAttack);

        PlayerFighterBase playerTarget = target != null ? target.GetComponent<PlayerFighterBase>() : null;
        playerTarget?.BeginParryChallenge(specialAttackDuration);

        yield return new WaitForSeconds(specialAttackDuration);

        bool parried = playerTarget != null && playerTarget.ResolveParryChallenge();
        Debug.Log($"[{name}] window parry abis -> hasil: {(parried ? "PLAYER BERHASIL PARRY" : "PLAYER GAGAL PARRY")}");

        CombatFreezeSignal.End(); // Lepas freeze SEBELUM damage diterapkan, biar reaksi kena damage keliatan normal (gak ke-freeze)

        if (!parried && playerTarget != null)
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