using System.Collections;
using UnityEngine;

public class MageProjectile
 : FighterBase
{
    [Header("Mage Specifics")]
    public GameObject projectilePrefab;
    public Transform shootPoint;

    // ==========================================
    // OVERRIDE ALUR SERANGAN (Tanpa Lari)
    // ==========================================
    protected override IEnumerator AttackFlow()
    {
        isAttacking = true;

        while (true)
        {
            // STATE 1: WIND-UP
            anim.Play(animAttack);

            yield return new WaitForSeconds(attackWindUpTime);

            // STATE 2: TEMBAK
            OnHitImpact();

            yield return new WaitForSeconds(hitStopTime);

            // STATE 3: CEK BUFFER KETIKAN
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
    // OVERRIDE EFEK SERANGAN (Spawn Projectile)
    // ==========================================
    protected override void OnHitImpact()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // Buat peluru
            GameObject peluru = Instantiate(projectilePrefab, shootPoint.position, Quaternion.identity);

            // Panggil script di peluru itu agar meluncur ke arah opponent
            // peluru.GetComponent<ProjectileController>().SetTarget(opponent, currDamage);
        }
    }
}