using UnityEngine;

public class TypingCombatBridge : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerFighterBase fighter;

    public bool IsPaused => CombatFreezeSignal.IsFrozen;

    private void Awake()
    {
        if (fighter == null) fighter = GetComponent<PlayerFighterBase>();
    }

    public void Attack()
    {
        if (fighter == null || fighter.IsDead) return;
        fighter.TriggerAttack();
    }

    public void SetCombo(int combo)
    {
        if (fighter == null || fighter.IsDead) return;
        fighter.ApplyTypingComboEffect(combo > 0);
    }
}