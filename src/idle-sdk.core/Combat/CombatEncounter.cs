namespace IdleSdk.Core.Combat;

public sealed class CombatEncounter
{
    private readonly List<CombatantState> _combatants;
    private readonly ICombatAi? _combatAi;
    private readonly long? _decisionSeed;

    public CombatEncounter(IEnumerable<CombatantState> combatants, ICombatAi? combatAi = null, CombatRules? rules = null, long? decisionSeed = null)
    {
        _combatants = combatants?.ToList() ?? throw new ArgumentNullException(nameof(combatants));
        if (_combatants.Count < 2)
        {
            throw new ArgumentException("At least two combatants are required.", nameof(combatants));
        }

        _combatAi = combatAi;
        Rules = rules ?? CombatRules.Default;
        if (Rules.MinDamage < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rules), "Minimum damage must be non-negative.");
        }

        if (Rules.MaxDamage < Rules.MinDamage)
        {
            throw new ArgumentOutOfRangeException(nameof(rules), "Maximum damage must be >= minimum damage.");
        }
        _decisionSeed = decisionSeed;
    }

    public IReadOnlyList<CombatantState> Combatants => _combatants;
    public CombatRules Rules { get; }

    public CombatTickResult Tick()
    {
        var logs = new List<CombatLogEntry>();
        var decisions = new List<CombatAiDecision>();
        foreach (var combatant in _combatants)
        {
            combatant.Effects.Tick();
        }

        for (var i = 0; i < _combatants.Count; i++)
        {
            var attacker = _combatants[i];
            if (attacker.IsDefeated)
            {
                continue;
            }

            var candidates = _combatants.Where(c => !c.IsDefeated && c != attacker).ToList();
            if (candidates.Count == 0)
            {
                break;
            }

            CombatantState? target = null;
            if (_combatAi is not null)
            {
                try
                {
                    target = _combatAi.SelectTarget(attacker, _combatants);
                }
                catch (InvalidOperationException)
                {
                    target = null;
                }
            }

            target ??= candidates.FirstOrDefault();
            if (target is null)
            {
                break;
            }

            if (_combatAi is not null)
            {
                decisions.Add(new CombatAiDecision(attacker.Id, target.Id, _decisionSeed));
            }

            var attackerTotals = attacker.Effects.GetTotals(Rules.EffectStacking);
            var targetTotals = target.Effects.GetTotals(Rules.EffectStacking);
            var attackPower = attacker.Stats.AttackPower + Rules.AttackBonus + attackerTotals.AttackDelta;
            var defense = target.Stats.Defense + Rules.DefenseBonus + targetTotals.DefenseDelta;
            var rawDamage = Math.Max(0, attackPower - defense);
            var damage = Math.Clamp(rawDamage, Rules.MinDamage, Rules.MaxDamage);
            target.ApplyDamage(damage);
            logs.Add(new CombatLogEntry(attacker.Id, target.Id, damage, target.IsDefeated));
        }

        return new CombatTickResult(logs, decisions);
    }
}
