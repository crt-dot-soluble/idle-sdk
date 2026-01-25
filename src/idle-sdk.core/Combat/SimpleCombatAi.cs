namespace IdleSdk.Core.Combat;

public sealed class SimpleCombatAi : ICombatAi
{
	public CombatantState SelectTarget(CombatantState attacker, IReadOnlyList<CombatantState> combatants)
	{
		var target = combatants.FirstOrDefault(c => !c.IsDefeated && c != attacker);
		if (target is null)
		{
			throw new InvalidOperationException("No available targets.");
		}

		return target;
	}
}
