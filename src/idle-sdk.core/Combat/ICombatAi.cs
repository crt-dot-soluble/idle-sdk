namespace IdleSdk.Core.Combat;

public interface ICombatAi
{
	CombatantState SelectTarget(CombatantState attacker, IReadOnlyList<CombatantState> combatants);
}
