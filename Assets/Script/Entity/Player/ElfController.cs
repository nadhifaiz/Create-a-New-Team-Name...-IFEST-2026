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

            // Panggil script di projectile-nya agar meluncur ke arah opponent
            // petir.GetComponent<ProjectileController>().SetTarget(opponent, currDamage);
        }
    }
}