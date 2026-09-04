using System.Collections;
using UnityEngine;

public class DragonController : EnemyFighterBase
{
    [Header("Dragon Specifics")]
    public GameObject fireballPrefab;
    public Transform shootPoint;

    protected override IEnumerator AttackFlow()
    {
        return ProjectileAttackFlow();
    }

    protected override void OnAttackStarted()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_NagaGrowl);
        }
    }

    protected override void ApplyImpactEffect()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_NagaNyemburinApi);
            PlaySfx(AudioSystem.Instance.sfx_Api);
        }

        if (fireballPrefab != null && shootPoint != null)
        {
            GameObject api = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);

            ProjectileController controller = api.GetComponent<ProjectileController>();
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
            PlaySfx(AudioSystem.Instance.sfx_NagaGrowl);
            PlaySfx(AudioSystem.Instance.sfx_NagaNyemburinApi);
        }
    }

    protected override void OnSpecialAttackLanded(PlayerFighterBase playerTarget)
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_Kebakar);
        }
    }
}
