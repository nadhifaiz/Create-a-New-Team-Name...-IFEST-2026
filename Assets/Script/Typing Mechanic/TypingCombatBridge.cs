using UnityEngine;

/// <summary>
/// Connects the typing minigame (TypeForward) to the combat system (FighterBase).
/// Deliberately NOT inside the TypeForward namespace/folder — it's the only class
/// allowed to know about both sides, so neither module has to reference the other.
///
/// SETUP:
/// 1. Put this on the same GameObject as your PlayerFighterBase (Knight/Elf/etc), or anywhere.
/// 2. Drag the PlayerFighterBase reference in the Inspector.
/// 3. On your TypeForwardController component:
///    - "On Sentence Complete" -> drag this object -> TypingCombatBridge.Attack()
///    - "On Combo Changed" (int) -> drag this object -> TypingCombatBridge.SetCombo(int)
/// </summary>
public class TypingCombatBridge : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerFighterBase fighter;

    private void Awake()
    {
        if (fighter == null) fighter = GetComponent<PlayerFighterBase>();
    }

    /// <summary>Wire to TypeForwardController.OnSentenceComplete.</summary>
    public void Attack()
    {
        if (fighter == null || fighter.IsDead) return;
        fighter.TriggerAttack();
    }

    /// <summary>Wire to TypeForwardController.OnComboChanged (int).</summary>
    public void SetCombo(int combo)
    {
        if (fighter == null || fighter.IsDead) return;
        fighter.ApplyTypingComboEffect(combo > 0);
    }
}