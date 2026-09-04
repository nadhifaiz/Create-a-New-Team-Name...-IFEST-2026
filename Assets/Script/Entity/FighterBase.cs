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
    [SerializeField] protected SpriteRenderer spriteRenderer;

    [Header("Damage Flash Effect")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;
    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;

    [Header("Death")]
    [SerializeField] protected bool destroyOnDeath = false;
    [SerializeField] protected float destroyDelay = 2f;

    // -- CACHE ANIMATION HASH (Hanya Idle, Attack, Die) --
    protected readonly int animIdle = Animator.StringToHash("Idle");
    protected readonly int animAttack = Animator.StringToHash("Attack");
    protected readonly int animDie = Animator.StringToHash("Die");
    protected const float crossFadeDuration = 0.1f;

    protected int currHP;
    protected int currDamage;
    protected Vector2 startPosition;

    protected int attackBuffer = 0;

    protected Transform currentTarget;
    protected Coroutine currentAttackCoroutine;

    // State Flags 
    protected bool isDead = false;
    public bool isAttacking = false;

    public bool IsDead => isDead;

    protected virtual void Start()
    {
        currHP = maxHP;
        currDamage = baseDamage;
        startPosition = transform.position;

        if (anim == null) anim = GetComponent<Animator>();

        // Ambil SpriteRenderer otomatis jika belum dicolok di Inspector
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
    }

    public void TriggerAttack()
    {
        if (isDead) return;

        if (isAttacking)
        {
            attackBuffer++;
        }
        else
        {
            if (CombatFreezeSignal.IsFrozen)
            {
                attackBuffer++;
                return;
            }

            currentTarget = AcquireTarget();
            if (currentTarget == null) return;

            currentAttackCoroutine = StartCoroutine(AttackFlow());
        }
    }

    public void ForceIdleAndBuffer()
    {
        if (isDead) return;

        if (isAttacking)
        {
            if (currentAttackCoroutine != null) StopCoroutine(currentAttackCoroutine);

            isAttacking = false;
            attackBuffer++;
            transform.position = startPosition;
        }

        PlayAnim(animIdle);
    }

    // ==========================================
    // CARI TARGET TERDEKAT YANG MASIH HIDUP
    // ==========================================
    protected Transform AcquireTarget()
    {
        if (opponent == null || opponent.Length == 0) return null;

        Transform nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Transform candidate in opponent)
        {
            if (candidate == null) continue;

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
    // ALUR SERANGAN — MELEE DASH
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
            if (currentTarget == null) break;

            // STATE 1: DASH MAJU (Tetap dalam pose Idle)
            PlayAnim(animIdle);

            while (Vector2.Distance(transform.position, currentTarget.position) > strikeDistance + 0.1f)
            {
                if (isDead || currentTarget == null) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null;
                    continue;
                }

                Vector2 direction = (currentTarget.position - transform.position).normalized;
                Vector2 targetPos = (Vector2)currentTarget.position - (direction * strikeDistance);

                transform.position = Vector2.MoveTowards(transform.position, targetPos, dashSpeed * Time.deltaTime);
                yield return null;
            }

            // STATE 2: ATTACK ANIMATION & WIND-UP
            PlayAnim(animAttack);

            float timer = attackWindUpTime;
            while (timer > 0)
            {
                if (isDead || currentTarget == null) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null;
                    continue;
                }

                timer -= Time.deltaTime;

                Vector2 dir = (currentTarget.position - transform.position).normalized;
                Vector2 microTarget = (Vector2)currentTarget.position - (dir * strikeDistance);
                transform.position = Vector2.MoveTowards(transform.position, microTarget, (dashSpeed * 0.5f) * Time.deltaTime);

                yield return null;
            }

            // STATE 3: IMPACT & HIT-STOP
            OnHitImpact();
            yield return new WaitForSeconds(hitStopTime);

            // STATE 4: CEK BUFFER (CHAIN ATTACK)
            if (attackBuffer > 0)
            {
                attackBuffer--;
                continue;
            }

            // STATE 5: MUNDUR KE POSISI AWAL (Kembali pose Idle)
            bool returnInterrupted = false;
            PlayAnim(animIdle);

            while (Vector2.Distance(transform.position, startPosition) > 0.05f)
            {
                if (isDead) yield break;

                if (CombatFreezeSignal.IsFrozen)
                {
                    yield return null;
                    continue;
                }

                if (attackBuffer > 0)
                {
                    attackBuffer--;
                    returnInterrupted = true;
                    break;
                }

                transform.position = Vector2.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
                yield return null;
            }

            if (returnInterrupted) continue;

            break;
        }

        // STATE 6: RESET
        transform.position = startPosition;
        isAttacking = false;
        PlayAnim(animIdle);
    }

    // ==========================================
    // ALUR SERANGAN PROJECTILE
    // ==========================================
    protected IEnumerator ProjectileAttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            if (currentTarget == null) break;

            PlayAnim(animAttack);
            yield return new WaitForSeconds(attackWindUpTime);

            while (CombatFreezeSignal.IsFrozen) yield return null;

            OnHitImpact();
            yield return new WaitForSeconds(hitStopTime);

            if (attackBuffer > 0)
            {
                attackBuffer--;
                continue;
            }

            break;
        }

        isAttacking = false;
        PlayAnim(animIdle);
    }

    protected virtual void OnHitImpact()
    {
        ApplyImpactEffect();
    }

    protected virtual void ApplyImpactEffect()
    {
        if (currentTarget != null)
        {
            FighterBase target = currentTarget.GetComponent<FighterBase>();
            if (target != null) target.TakeDamage(currDamage);
        }
    }

    // ==========================================
    // TERKENA DAMAGE & FLASH EFFECT
    // ==========================================
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        ApplyDamage(damage);

        if (!isDead)
        {
            // Flash merah sekejap tanpa merusak animasi karakter
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hitFlashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

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

        // Kembalikan warna normal saat gugur
        if (spriteRenderer != null) spriteRenderer.color = originalColor;

        PlayAnim(animDie);
        Debug.Log($"[{name}] MATI");

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    protected void PlayAnim(int animHash)
    {
        if (anim != null)
        {
            anim.CrossFade(animHash, crossFadeDuration);
        }
    }
}