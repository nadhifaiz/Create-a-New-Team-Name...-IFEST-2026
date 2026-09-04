using System.Collections;
using UnityEngine;

// Elf — input/combo dari PlayerFighterBase, tapi cara serangnya projectile
// (lightning), jadi override AttackFlow + ApplyImpactEffect.
// Set comboFirstBonus = 5, comboRepeatBonus = 5 di Inspector (flat tiap combo).
public class ElfController : PlayerFighterBase
{
    [Header("Elf Specifics")]
    public GameObject lightningPrefab;
    public Transform castPoint;

    protected override IEnumerator AttackFlow()
    {
        return ProjectileAttackFlow();
    }

    protected override void ApplyImpactEffect()
    {
        if (lightningPrefab != null && castPoint != null)
        {
            GameObject petir = Instantiate(lightningPrefab, castPoint.position, Quaternion.identity);

            ProjectileController controller = petir.GetComponent<ProjectileController>();
            if (controller != null) controller.SetTarget(currentTarget, currDamage);
        }
    }
}