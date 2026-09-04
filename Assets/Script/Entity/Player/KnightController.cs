public class KnightController : PlayerFighterBase
{
    protected override void OnHitImpact()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_TancapPedangKeTanah);
        }

        base.OnHitImpact();
    }

    protected override void Die()
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_PedangPlayerHilang);
        }

        base.Die();
    }
}
