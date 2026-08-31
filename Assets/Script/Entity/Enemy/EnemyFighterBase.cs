using System.Collections;
using UnityEngine;

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

    private bool nextAttackIsMiss = false;
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
            float waitTime = Random.Range(minAttackInterval, maxAttackInterval);
            yield return new WaitForSeconds(waitTime);

            if (isDead) yield break;
            if (isAttacking) continue;

            float missRoll = Random.value;
            nextAttackIsMiss = missRoll < missChance;

            TriggerAttack();
        }
    }

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

        // Cari target sendiri — bukan lewat TriggerAttack() biasa, jadi
        // currentTarget yang lama (kalau ada) gak dipakai di sini.
        Transform target = AcquireTarget();
        anim.Play(animSpecialAttack);

        PlayerFighterBase playerTarget = target != null ? target.GetComponent<PlayerFighterBase>() : null;
        playerTarget?.BeginParryChallenge(specialAttackDuration);

        yield return new WaitForSeconds(specialAttackDuration);

        bool parried = playerTarget != null && playerTarget.ResolveParryChallenge();
        Debug.Log($"[{name}] window parry abis -> hasil: {(parried ? "PLAYER BERHASIL PARRY" : "PLAYER GAGAL PARRY")}");

        if (!parried && playerTarget != null)
        {
            int specialDamage = Mathf.RoundToInt(currDamage * specialDamageMultiplier);
            playerTarget.TakeSpecialDamage(specialDamage);
        }

        anim.Play(animIdle);
        isAttacking = false;
    }
}