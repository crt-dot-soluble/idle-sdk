using IdleSdk.Core.Combat;

namespace IdleSdk.Core.Tests.Combat;

public class CombatSystemTests
{
    [Fact]
    public void CombatTick_Applies_Damage_And_Logs()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy", new CombatantStats(8, 3, 0))
        };

        var encounter = new CombatEncounter(combatants);
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.NotEmpty(result.LogEntries);
        Assert.Equal(2, result.LogEntries.Count);
        Assert.Empty(result.AiDecisions);
        Assert.Equal(3, combatants[1].CurrentHealth);
    }

    [Fact]
    public void CombatTick_Stops_When_Only_One_Left()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 50, 0)),
            new("enemy", new CombatantStats(5, 1, 0))
        };

        var encounter = new CombatEncounter(combatants);
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.Single(result.LogEntries);
        Assert.Empty(result.AiDecisions);
        Assert.True(combatants[1].IsDefeated);
    }

    [Fact]
    public void CombatTick_Applies_Status_Effects()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy", new CombatantStats(8, 3, 0))
        };

        combatants[0].Effects.Add(new StatusEffect("buff", 2, 2, 0));
        var encounter = new CombatEncounter(combatants);
        var system = new CombatSystem();

        system.Step(encounter);

        Assert.Equal(1, combatants[1].CurrentHealth);
    }

    [Fact]
    public void CombatTick_Uses_Ai_Targeting()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy1", new CombatantStats(8, 3, 0)),
            new("enemy2", new CombatantStats(8, 3, 0))
        };

        var encounter = new CombatEncounter(combatants, new FocusSecondEnemyAi());
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.Equal(3, combatants[2].CurrentHealth);
        Assert.Equal(8, combatants[1].CurrentHealth);
        Assert.NotEmpty(result.AiDecisions);
    }

    [Fact]
    public void CombatTick_Does_Not_Throw_When_No_Targets()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy", new CombatantStats(1, 1, 0))
        };

        combatants[1].ApplyDamage(999);
        var encounter = new CombatEncounter(combatants, new SimpleCombatAi());
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.Empty(result.LogEntries);
    }

    [Fact]
    public void CombatTick_Respects_Damage_Cap()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 10, 0)),
            new("enemy", new CombatantStats(20, 1, 0))
        };

        var encounter = new CombatEncounter(combatants, null, new CombatRules(0, 2, 0, 0, EffectStackingMode.Additive));
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.Equal(2, result.LogEntries[0].Damage);
    }

    [Fact]
    public void CombatTick_Uses_Max_Stacking()
    {
        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy", new CombatantStats(8, 3, 0))
        };

        combatants[0].Effects.Add(new StatusEffect("buff-1", 2, 1, 0));
        combatants[0].Effects.Add(new StatusEffect("buff-2", 2, 3, 0));

        var encounter = new CombatEncounter(combatants, null, new CombatRules(0, int.MaxValue, 0, 0, EffectStackingMode.Max));
        var system = new CombatSystem();

        var result = system.Step(encounter);

        Assert.Equal(8, result.LogEntries[0].Damage);
    }

    [Fact]
    public void CombatantEffects_Roundtrip_Snapshot()
    {
        var effects = new CombatantEffects();
        effects.Add(new StatusEffect("buff", 2, 1, 0));

        var snapshot = effects.GetSnapshot();
        var restored = new CombatantEffects();
        restored.RestoreSnapshot(snapshot);

        Assert.Single(restored.Effects);
        Assert.Equal("buff", restored.Effects.First().Id);
    }

    private sealed class FocusSecondEnemyAi : ICombatAi
    {
        public CombatantState SelectTarget(CombatantState attacker, IReadOnlyList<CombatantState> combatants)
        {
            if (attacker.Id == "player")
            {
                return combatants.First(c => c.Id == "enemy2");
            }

            return combatants.First(c => c.Id == "player");
        }
    }
}
