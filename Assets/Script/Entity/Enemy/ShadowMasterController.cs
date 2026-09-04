using System.Collections;
using UnityEngine;

// ShadowMaster — AI (interval random + miss chance) dari EnemyFighterBase,
// tapi cara serangnya projectile (shadow bolt), jadi override
// AttackFlow + ApplyImpactEffect.
public class ShadowMasterController : EnemyFighterBase
{
    [Header("ShadowMaster Specifics")]
    public GameObject shadowBoltPrefab;
    public Transform castPoint;

    protected override IEnumerator AttackFlow()
    {
        return ProjectileAttackFlow();
    }

    protected override void ApplyImpactEffect()
    {
        if (shadowBoltPrefab != null && castPoint != null)
        {
            GameObject bola = Instantiate(shadowBoltPrefab, castPoint.position, Quaternion.identity);

            ProjectileController controller = bola.GetComponent<ProjectileController>();
            if (controller != null) controller.SetTarget(currentTarget, currDamage);
        }
    }
}