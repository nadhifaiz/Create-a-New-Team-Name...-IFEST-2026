using System.Collections;
using UnityEngine;

public class ShadowMasterController : EnemyFighterBase
{
    [Header("ShadowMaster Specifics")]
    public GameObject shadowBoltPrefab;
    public Transform castPoint;

    protected override IEnumerator AttackFlow()
    {
        return ProjectileAttackFlow();
    }

    protected override void OnAttackStarted()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_Hantu);
        }
    }

    protected override void ApplyImpactEffect()
    {
        if (shadowBoltPrefab != null && castPoint != null)
        {
            GameObject bola = Instantiate(shadowBoltPrefab, castPoint.position, Quaternion.identity);

            ProjectileController controller = bola.GetComponent<ProjectileController>();
            if (controller != null)
            {
                controller.SetTarget(currentTarget, currDamage);
            }
        }
    }

    protected override void OnSpecialAttackStarted()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_Hantu);
        }
    }

    protected override void OnSpecialAttackLanded(PlayerFighterBase playerTarget)
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_HantuMakanPlayer);
        }
    }
}
