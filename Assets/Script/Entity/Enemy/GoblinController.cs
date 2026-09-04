public class GoblinController : EnemyFighterBase
{
    protected override void OnAttackStarted()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_Goblin);
        }
    }

    protected override void OnHitImpact()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_BattonSwingGoblin);
        }

        base.OnHitImpact();
    }

    protected override void OnSpecialAttackStarted()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_SuaraTanahKebelah);
        }
    }
}
