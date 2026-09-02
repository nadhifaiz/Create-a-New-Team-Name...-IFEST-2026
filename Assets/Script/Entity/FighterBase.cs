using System.Collections;
using UnityEngine;

public abstract class FighterBase : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected int maxHP = 100;
    [SerializeField] protected int baseDamage = 10;

    [Header("Combat Setting")]
    public Transform[] opponent;
    public float strikeDistance = 1.2f;
    public float dashSpeed = 25f;
    public float returnSpeed = 15f;
    [SerializeField] protected float attackWindUpTime = 0.1f;
    [SerializeField] protected float hitStopTime = 0.05f;

    [Header("Components")]
    [SerializeField] protected Animator anim;

    [Header("Death")]
    [SerializeField] protected bool destroyOnDeath = false;
    [SerializeField] protected float destroyDelay = 2f;

    // -- CACHE ANIMATION HASH (GLOBAL — dipakai player & enemy) --
    protected readonly int animIdle = Animator.StringToHash("Idle");
    protected readonly int animMove = Animator.StringToHash("Move");
    protected readonly int animAttack = Animator.StringToHash("Attack");
    protected readonly int animHit = Animator.StringToHash("Hit");   // reaksi kena damage NORMAL (global)
    protected readonly int animDie = Animator.StringToHash("Die");

    protected int currHP;
    protected int currDamage;
    protected Vector2 startPosition;

    protected int attackBuffer = 0;

    protected Transform currentTarget;

    // State Flags 
    protected bool isDead = false;
    public bool isAttacking = false;

    public bool IsDead => isDead; // dipakai AcquireTarget buat skip target yang udah mati

    protected virtual void Start()
    {
        currHP = maxHP;
        currDamage = baseDamage;
        startPosition = transform.position;

        if (anim == null) anim = GetComponent<Animator>();
    }

    public void TriggerAttack()
    {
        if (isDead) return;

        if (isAttacking)
        {
            // Tambah antrean jika sedang sibuk memukul
            attackBuffer++;
        }
        else
        {
            // Lagi ada special attack QTE di tempat lain -> semua serangan baru ditahan dulu
            if (CombatFreezeSignal.IsFrozen) return;

            // Cari target terdekat yang masih hidup dari daftar opponent
            currentTarget = AcquireTarget();
            if (currentTarget == null) return; // gak ada target hidup, batal nyerang

            // Mulai serangan jika sedang nganggur
            StartCoroutine(AttackFlow());
        }
    }

    // ==========================================
    // CARI TARGET TERDEKAT YANG MASIH HIDUP
    // Dipakai buat kasus 1 vs banyak (misal 1 player vs 3 goblin)
    // ==========================================
    protected Transform AcquireTarget()
    {
        if (opponent == null || opponent.Length == 0) return null;

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Transform candidate in opponent)
        {
            if (candidate == null) continue; // misalnya udah di-Destroy

            FighterBase candidateFighter = candidate.GetComponent<FighterBase>();
            if (candidateFighter != null && candidateFighter.IsDead) continue;

            float dist = Vector2.Distance(transform.position, candidate.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = candidate;
            }
        }

        return nearest;
    }

    // ==========================================
    // ALUR SERANGAN — DEFAULT-NYA MELEE DASH
    // Karakter projectile (Elf/Dragon/ShadowMaster) override ini
    // dan cukup delegasikan ke ProjectileAttackFlow()
    // ==========================================
    protected virtual IEnumerator AttackFlow()
    {
        return MeleeAttackFlow();
    }

    protected IEnumerator MeleeAttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            if (currentTarget == null) break; // target keburu mati/ilang

            // ==========================================
            // STATE 1: DASH MAJU
            // ==========================================
            anim.Play(animMove);

            while (Vector2.Distance(transform.position, currentTarget.position) > strikeDistance + 0.1f)
            {
                if (isDead || currentTarget == null) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null; // ada special attack lagi jalan -> tahan di tempat
                    continue;
                }

                Vector2 direction = (currentTarget.position - transform.position).normalized;
                Vector2 targetPos = (Vector2)currentTarget.position - (direction * strikeDistance);

                transform.position = Vector2.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
                yield return null;
            }

            // ==========================================
            // STATE 2: WIND-UP & MICRO-TRACKING
            // ==========================================
            anim.Play(animAttack);

            float timer = attackWindUpTime;
            while (timer > 0)
            {
                if (isDead || currentTarget == null) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null; // timer gak jalan selama freeze
                    continue;
                }

                timer -= Time.deltaTime;

                // Terus menempel mengikuti pergerakan musuh
                Vector2 dir = (currentTarget.position - transform.position).normalized;
                Vector2 microTarget = (Vector2)currentTarget.position - (dir * strikeDistance);
                transform.position = Vector2.MoveTowards(transform.position, microTarget, (dashSpeed * 0.5f) * Time.deltaTime);

                yield return null;
            }

            // ==========================================
            // STATE 3: IMPACT & HIT-STOP
            // ==========================================
            OnHitImpact();
            yield return new WaitForSeconds(hitStopTime);

            // ==========================================
            // STATE 4: CEK BUFFER (CHAIN ATTACK)
            // ==========================================
            if (attackBuffer > 0)
            {
                attackBuffer--;
                continue; // Langsung hajar lagi (target yang sama)
            }

            // ==========================================
            // STATE 5: MUNDUR KE POSISI AWAL
            // ==========================================
            bool returnInterrupted = false;
            anim.Play(animMove);

            while (Vector2.Distance(transform.position, startPosition) > 0.05f)
            {
                if (isDead) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null;
                    continue;
                }

                // Ngerem mendadak jika tiba-tiba ada input baru
                if (attackBuffer > 0)
                {
                    attackBuffer--;
                    returnInterrupted = true;
                    break;
                }

                transform.position = Vector2.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
                yield return null;
            }

            // Putar balik untuk menyerang lagi
            if (returnInterrupted) continue;

            break;
        }

        // ==========================================
        // STATE 6: RESET
        // ==========================================
        transform.position = startPosition;
        isAttacking = false;
        anim.Play(animIdle);
    }

    // ==========================================
    // ALUR SERANGAN PROJECTILE (REUSABLE)
    // ==========================================
    protected IEnumerator ProjectileAttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            if (currentTarget == null) break;

            // STATE 1: WIND-UP
            anim.Play(animAttack);
            yield return new WaitForSeconds(attackWindUpTime);

            while (CombatFreezeSignal.IsFrozen) yield return null;

            // STATE 2: TEMBAK (delegasi ke ApplyImpactEffect via OnHitImpact)
            OnHitImpact();
            yield return new WaitForSeconds(hitStopTime);

            // STATE 3: CEK BUFFER
            if (attackBuffer > 0)
            {
                attackBuffer--;
                continue;
            }

            break;
        }

        // STATE 4: RESET
        isAttacking = false;
        anim.Play(animIdle);
    }

    // ==========================================
    // IMPACT — dipanggil di titik "kena" tiap AttackFlow
    // Enemy override ini buat nyisipin cek miss-chance
    // ==========================================
    protected virtual void OnHitImpact()
    {
        ApplyImpactEffect();
    }

    // ==========================================
    // EFEK SERANGAN — beda per tipe serangan (melee/projectile)
    // ==========================================
    protected virtual void ApplyImpactEffect()
    {
        if (currentTarget != null)
        {
            FighterBase target = currentTarget.GetComponent<FighterBase>();
            if (target != null) target.TakeDamage(currDamage);
        }
    }

    // ==========================================
    // TERKENA DAMAGE NORMAL (GLOBAL — dipakai player & enemy)
    // ==========================================
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        ApplyDamage(damage);

        if (!isDead)
        {
            anim.Play(animHit);
        }
    }

    // Dipisah dari TakeDamage biar bisa dipakai ulang sama varian damage lain
    // (misal TakeSpecialDamage di PlayerFighterBase) tanpa duplikat logic HP.
    protected void ApplyDamage(int damage)
    {
        currHP -= damage;
        Debug.Log($"[{name}] kena damage {damage} -> HP {currHP}/{maxHP}");

        if (currHP <= 0)
        {
            currHP = 0;
            Die();
        }
    }

    protected virtual void Die()
    {
        isDead = true;
        StopAllCoroutines();
        anim.Play(animDie);
        Debug.Log($"[{name}] MATI");

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
}