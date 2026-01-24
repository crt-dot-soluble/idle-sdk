using IdleSdk.Core.Skills;

namespace IdleSdk.Core.Tests.Skills;

public class SkillSystemTests
{
    [Fact]
    public void SkillProgress_Levels_Up_When_Xp_Reaches_Threshold()
    {
        var registry = new SkillRegistry();
        registry.Register(new SkillDefinition("mining", "Mining", 5));

        var curve = new LinearXpCurve(100, 50);
        var system = new SkillSystem(registry, curve);

        system.AddXp("mining", 100);

        var progress = system.GetOrCreateProgress("mining");
        Assert.Equal(2, progress.Level);

        system.AddXp("mining", 50);
        Assert.Equal(3, progress.Level);
    }

    [Fact]
    public void SkillProgress_Stops_At_Max_Level()
    {
        var registry = new SkillRegistry();
        registry.Register(new SkillDefinition("combat", "Combat", 2));

        var curve = new LinearXpCurve(10, 10);
        var system = new SkillSystem(registry, curve);

        system.AddXp("combat", 1000);

        var progress = system.GetOrCreateProgress("combat");
        Assert.Equal(2, progress.Level);
    }
}
