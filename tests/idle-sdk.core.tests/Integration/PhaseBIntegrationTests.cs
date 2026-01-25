using IdleSdk.Core.Actions;
using IdleSdk.Core.Combat;
using IdleSdk.Core.Skills;

namespace IdleSdk.Core.Tests.Integration;

public class PhaseBIntegrationTests
{
    [Fact]
    public void Action_Cooldown_Skill_Unlock_Combat_Effect_Integration()
    {
        var skillRegistry = new SkillRegistry();
        skillRegistry.Register(new SkillDefinition("gathering", "Gathering", 5, new Dictionary<int, string>
        {
            [2] = "Unlocked: Swift Gather"
        }));

        var skillSystem = new SkillSystem(skillRegistry, new LinearXpCurve(50, 25));

        var actionRegistry = new ActionRegistry();
        actionRegistry.RegisterDefinition(new ActionDefinition("gather", "Gather", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), new[] { "gather" }));
        actionRegistry.RegisterHandler(new GainXpHandler(skillSystem));

        var runner = new ActionRunner(actionRegistry);
        var start = DateTimeOffset.UtcNow;
        var first = runner.Step("gather", new ActionContext(Guid.NewGuid(), start), TimeSpan.FromSeconds(1));
        var second = runner.Step("gather", new ActionContext(Guid.NewGuid(), start.AddSeconds(1)), TimeSpan.FromSeconds(1));

        Assert.True(first.Completed);
        Assert.False(second.Completed);

        var progress = skillSystem.GetOrCreateProgress("gathering");
        Assert.Contains("Unlocked: Swift Gather", progress.Unlocks);

        var combatants = new List<CombatantState>
        {
            new("player", new CombatantStats(10, 5, 1)),
            new("enemy", new CombatantStats(8, 3, 0))
        };

        combatants[0].Effects.Add(new StatusEffect("focus", 2, 2, 0));
        var encounter = new CombatEncounter(combatants);
        var combatSystem = new CombatSystem();

        var result = combatSystem.Step(encounter);

        Assert.Equal(7, result.LogEntries[0].Damage);
    }

    private sealed class GainXpHandler : IActionHandler
    {
        private readonly SkillSystem _skillSystem;

        public GainXpHandler(SkillSystem skillSystem)
        {
            _skillSystem = skillSystem;
        }

        public string ActionId => "gather";

        public ActionResult Execute(ActionContext context, TimeSpan delta)
        {
            _skillSystem.AddXp("gathering", 100);
            return new ActionResult(true, "xp");
        }
    }
}
