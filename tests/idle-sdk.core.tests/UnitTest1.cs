using IdleSdk.Core;
using IdleSdk.Core.Events;
using IdleSdk.Core.Modules;

namespace IdleSdk.Core.Tests;

public class EngineSpineTests
{
    [Fact]
    public void SimulationClock_Advances_Deterministically()
    {
        var clock = new SimulationClock(2);

        var first = clock.Advance(TimeSpan.FromSeconds(1.1));
        var second = clock.Advance(TimeSpan.FromSeconds(0.4));

        Assert.Equal(2, first);
        Assert.Equal(1, second);
        Assert.Equal(3, clock.TotalTicks);
        Assert.Equal(TimeSpan.FromSeconds(1.5), clock.TotalSimulatedTime);
    }

    [Fact]
    public void EventHub_Publishes_To_Subscribers()
    {
        var hub = new EventHub();
        var count = 0;

        using var subscription = hub.Subscribe<string>(_ => count++);

        hub.Publish("one");
        hub.Publish("two");

        Assert.Equal(2, count);
    }

    [Fact]
    public void ModuleRegistry_Resolves_Dependency_Order()
    {
        var registry = new ModuleRegistry();

        registry.Register(new TestModule("B", "Module B", new Version(1, 0), Array.Empty<ModuleDependency>()));
        registry.Register(new TestModule("A", "Module A", new Version(1, 0), new[] { new ModuleDependency("B", new Version(1, 0)) }));

        var ordered = registry.ResolveLoadOrder();

        Assert.Equal(new[] { "B", "A" }, ordered.Select(m => m.Id));
    }

    private sealed class TestModule : IModule
    {
        public TestModule(string id, string name, Version version, IReadOnlyCollection<ModuleDependency> dependencies)
        {
            Id = id;
            Name = name;
            Version = version;
            Dependencies = dependencies;
        }

        public string Id { get; }
        public string Name { get; }
        public Version Version { get; }
        public IReadOnlyCollection<ModuleDependency> Dependencies { get; }

        public void Initialize(ModuleContext context)
        {
        }
    }
}
