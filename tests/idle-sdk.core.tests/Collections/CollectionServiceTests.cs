using IdleSdk.Core.Collections;

namespace IdleSdk.Core.Tests.Collections;

public class CollectionServiceTests
{
    [Fact]
    public void Collection_Completes_When_All_Items_Collected()
    {
        var registry = new CollectionRegistry();
        registry.Register(new CollectionDefinition("starter", "Starter Set", new[] { "log", "stone" }));

        var service = new CollectionService(registry);
        var profileId = Guid.NewGuid();

        service.AddItem(profileId, "starter", "log");
        service.AddItem(profileId, "starter", "stone");

        var progress = service.GetOrCreate(profileId, "starter");
        Assert.True(progress.IsComplete);
    }
}
