namespace IdleSdk.Core.Combat;

public sealed class CombatantEffects
{
	private readonly List<StatusEffect> _effects = new();

	public IReadOnlyCollection<StatusEffect> Effects => _effects.AsReadOnly();

	public void Add(StatusEffect effect)
	{
		if (effect is null)
		{
			throw new ArgumentNullException(nameof(effect));
		}

		_effects.Add(effect);
	}

	public IReadOnlyList<StatusEffect> GetSnapshot() => _effects.ToList();

	public void RestoreSnapshot(IEnumerable<StatusEffect> effects)
	{
		if (effects is null)
		{
			throw new ArgumentNullException(nameof(effects));
		}

		_effects.Clear();
		_effects.AddRange(effects);
	}

	public void Tick()
	{
		for (var i = _effects.Count - 1; i >= 0; i--)
		{
			var effect = _effects[i];
			if (effect.DurationTicks <= 1)
			{
				_effects.RemoveAt(i);
				continue;
			}

			_effects[i] = effect with { DurationTicks = effect.DurationTicks - 1 };
		}
	}

	public (int AttackDelta, int DefenseDelta) GetTotals(EffectStackingMode stackingMode)
	{
		if (_effects.Count == 0)
		{
			return (0, 0);
		}

		return stackingMode switch
		{
			EffectStackingMode.Max => (
				_effects.Max(effect => effect.AttackDelta),
				_effects.Max(effect => effect.DefenseDelta)),
			_ => (
				_effects.Sum(effect => effect.AttackDelta),
				_effects.Sum(effect => effect.DefenseDelta))
		};
	}
}
