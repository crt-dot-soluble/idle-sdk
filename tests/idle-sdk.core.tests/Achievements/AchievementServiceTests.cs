using IdleSdk.Core.Achievements;

namespace IdleSdk.Core.Tests.Achievements;

public class AchievementServiceTests
{
    [Fact]
    public void Achievement_Completes_When_Target_Reached()
    {
        var registry = new AchievementRegistry();
        registry.Register(new AchievementDefinition("kill10", "Slayer", "Defeat 10 enemies", 10));

        var service = new AchievementService(registry);
        var profileId = Guid.NewGuid();

        service.AddProgress(profileId, "kill10", 7);
        service.AddProgress(profileId, "kill10", 3);

        var progress = service.GetOrCreate(profileId, "kill10");
        Assert.True(progress.Completed);
        Assert.Equal(10, progress.CurrentValue);
    }
}
