using System.Collections;
using UnityEngine;

// Dragon — AI (interval random + miss chance) dari EnemyFighterBase,
// tapi cara serangnya projectile (fire breath), jadi override
// AttackFlow + ApplyImpactEffect.
public class DragonController : EnemyFighterBase
{
    [Header("Dragon Specifics")]
    public GameObject fireballPrefab;
    public Transform shootPoint;

    protected override IEnumerator AttackFlow()
    {
        return ProjectileAttackFlow();
    }

    protected override void ApplyImpactEffect()
    {
        if (fireballPrefab != null && shootPoint != null)
        {
            GameObject api = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);

            // Panggil script di projectile-nya agar meluncur ke arah opponent
            // api.GetComponent<ProjectileController>().SetTarget(opponent, currDamage);
        }
    }
}