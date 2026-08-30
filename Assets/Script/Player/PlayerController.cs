using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Setting")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int baseDamage = 10;
    [SerializeField] private int maxDamage = 80;

    [Header("MonkeyType Combat Setting")]
    public Transform opponent; // Posisi Musuh
    public float strikeDistance = 1.2f;
    public float dashSpeed = 25f;
    public float returnSpeed = 15f;
    [SerializeField] private float attackWindUpTime = 0.1f;
    [SerializeField] private float hitStopTime = 0.05f;

    [Header("Component Setting")]
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject comboAura;

    // -- CACHE ANIMATION HASH --
    private readonly int animIdle = Animator.StringToHash("Idle");
    private readonly int animAttack = Animator.StringToHash("Attack");
    private readonly int animParry = Animator.StringToHash("Parry");
    private readonly int animMove = Animator.StringToHash("Move");

    private int currHP;
    private int currDamage;
    private Vector2 startPosition;
    private int comboCount = 0; // Pastikan dimulai dari 0

    // Sistem Antrean Ketikan (Attack Buffer)
    private int attackBuffer = 0;

    // State Flags 
    private bool isDead = false;
    public bool isAttacking = false;
    public bool isParrying = false;
    public bool isTypingComboActive = false;

    void Start()
    {
        currHP = maxHP;
        currDamage = baseDamage;
        startPosition = transform.position;

        if (anim == null) anim = GetComponent<Animator>();

        if (comboAura != null) comboAura.SetActive(false);
    }

    void Update()
    {
        if (isDead) return;
        HandleCombatInput();
    }

    private void HandleCombatInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.jKey.wasPressedThisFrame && !isParrying)
        {
            TriggerAttack();
            ApplyTypingComboEffect(true);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame && !isAttacking && !isParrying)
        {
            Parry(true);
            ApplyTypingComboEffect(false);
            Parry(false);
        }
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
            StartCoroutine(AttackFlow());
        }
    }

    private IEnumerator AttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            // ==========================================
            // STATE 1: DASH 
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

            // STATE 2: WIND-UP & MICRO-TRACKING
            anim.Play(animAttack);

            float timer = attackWindUpTime;
            while (timer > 0)
            {
                if (isDead) yield break;
                timer -= Time.deltaTime;

                Vector2 dir = (opponent.position - transform.position).normalized;
                Vector2 microTarget = (Vector2)opponent.position - (dir * strikeDistance);
                transform.position = Vector2.MoveTowards(transform.position, microTarget, (dashSpeed * 0.5f) * Time.deltaTime);

                yield return null;
            }

            // STATE 3: IMPACT 
            yield return new WaitForSeconds(hitStopTime);

            // STATE 4: CEK BUFFER 
            if (attackBuffer > 0)
            {
                attackBuffer--;
                continue;
            }

            // ==========================================
            // STATE 5: MUNDUR 
            // ==========================================
            bool returnInterrupted = false;

            anim.Play(animMove);

            while (Vector2.Distance(transform.position, startPosition) > 0.05f)
            {
                if (isDead) yield break;

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
        anim.Play(animIdle);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isParrying) return;

        currHP -= damage;
        if (currHP <= 0)
        {
            currHP = 0;
            Die();
        }
    }

    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
        if (comboAura != null) comboAura.SetActive(false);
        Debug.Log("Player Mati!");
    }

    public void Parry(bool parrying)
    {
        if (isDead) return;
        isParrying = parrying;
        anim.Play(animParry); // menyesuaikan animasi unparry dan parry aja sih ini.
    }

    // ==========================================
    // SISTEM KOMBO KETIKAN (REFACTORED)
    // ==========================================
    public void ApplyTypingComboEffect(bool isCombo)
    {
        isTypingComboActive = isCombo;

        if (isCombo)
        {
            // Tambah hitungan kombo jika berhasil ngetik
            comboCount++;

            if (comboAura != null) comboAura.SetActive(true);

            IncreaseDamage(3);
        }
        else
        {
            // Jika salah ketik / kombo putus, reset semua ke awal
            comboCount = 0;
            if (comboAura != null) comboAura.SetActive(false);
            ResetDamageToBase();
        }
    }

    private void IncreaseDamage(int amount)
    {
        int temp = currDamage + amount;

        // Memastikan damage mentok di angka maxDamage (80)
        if (temp > maxDamage)
        {
            currDamage = maxDamage;
        }
        else
        {
            currDamage = temp;
        }
    }

    private void ResetDamageToBase()
    {
        currDamage = baseDamage;
    }
}