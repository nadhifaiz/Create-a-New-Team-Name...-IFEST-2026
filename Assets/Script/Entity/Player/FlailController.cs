public class FlailController : PlayerFighterBase
{
    public override void BeginParryChallenge(float duration)
    {
        if (AudioSystem.Instance != null)
        {
            PlaySfx(AudioSystem.Instance.sfx_IceShield);
        }

        base.BeginParryChallenge(duration);
    }
}
