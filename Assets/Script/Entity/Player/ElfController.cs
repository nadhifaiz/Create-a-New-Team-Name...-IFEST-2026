using System.Collections;
using UnityEngine;

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
            if (controller != null)
            {
                controller.SetTarget(currentTarget, currDamage);
            }
        }
    }

    public override void BeginParryChallenge(float duration)
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_PenyihirBuatPerisai);
        }

        base.BeginParryChallenge(duration);
    }
}
