using System.Collections;
using UnityEngine;

public abstract class FighterBase : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] protected int maxHP = 100;
    [SerializeField] protected int baseDamage = 10;

    [Header("Combat Setting")]
    public Transform opponent;
    public float strikeDistance = 1.2f;
    public float dashSpeed = 25f;
    public float returnSpeed = 15f;
    [SerializeField] protected float attackWindUpTime = 0.1f;
    [SerializeField] protected float hitStopTime = 0.05f;

    [Header("Components")]
    [SerializeField] protected Animator anim;

    // -- CACHE ANIMATION HASH --
    protected readonly int animIdle = Animator.StringToHash("Idle");
    protected readonly int animMove = Animator.StringToHash("Move");
    protected readonly int animAttack = Animator.StringToHash("Attack");

    protected int currHP;
    protected int currDamage;
    protected Vector2 startPosition;

    protected int attackBuffer = 0;

    // State Flags 
    protected bool isDead = false;
    public bool isAttacking = false;

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
            // Mulai serangan jika sedang nganggur
            StartCoroutine(AttackFlow());
        }
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
            // ==========================================
            // STATE 1: DASH MAJU
            // ==========================================
            anim.Play(animMove);

            while (Vector2.Distance(transform.position, opponent.position) > strikeDistance + 0.1f)
            {
                if (isDead) yield break;

                Vector2 direction = (opponent.position - transform.position).normalized;
                Vector2 targetPos = (Vector2)opponent.position - (direction * strikeDistance);

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
                if (isDead) yield break;
                timer -= Time.deltaTime;

                // Terus menempel mengikuti pergerakan musuh
                Vector2 dir = (opponent.position - transform.position).normalized;
                Vector2 microTarget = (Vector2)opponent.position - (dir * strikeDistance);
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
                continue; // Langsung hajar lagi
            }

            // ==========================================
            // STATE 5: MUNDUR KE POSISI AWAL
            // ==========================================
            bool returnInterrupted = false;
            anim.Play(animMove);

            while (Vector2.Distance(transform.position, startPosition) > 0.05f)
            {
                if (isDead) yield break;

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
    // Dipakai oleh Elf, Dragon, ShadowMaster — tinggal:
    //   protected override IEnumerator AttackFlow() => ProjectileAttackFlow();
    // Gak perlu dash, cuma wind-up di tempat lalu tembak.
    // ==========================================
    protected IEnumerator ProjectileAttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            // STATE 1: WIND-UP
            anim.Play(animAttack);
            yield return new WaitForSeconds(attackWindUpTime);

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
    // Enemy override ini buat nyisipin cek miss-chance,
    // lalu tetap panggil ApplyImpactEffect() lewat base.OnHitImpact()
    // ==========================================
    protected virtual void OnHitImpact()
    {
        ApplyImpactEffect();
    }

    // ==========================================
    // EFEK SERANGAN — INI YANG BEDA PER TIPE SERANGAN
    // Default: kasih damage langsung ke opponent (melee).
    // Karakter projectile override ini buat spawn projectile.
    // ==========================================
    protected virtual void ApplyImpactEffect()
    {
        if (opponent != null)
        {
            FighterBase target = opponent.GetComponent<FighterBase>();
            if (target != null) target.TakeDamage(currDamage);
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

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
        Debug.Log($"[{name}] MATI");
    }
}