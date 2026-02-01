using IdleSdk.Core.Events;
using IdleSdk.Core.Modules;

namespace IdleSdk.Core.Tests.Modules;

public class ModuleLoaderTests
{
    [Fact]
    public void LoadAll_Initializes_Modules_In_Dependency_Order()
    {
        var eventsHub = new EventHub();
        var registry = new ModuleRegistry();
        var order = new List<string>();

        var core = new TestModule("core", "Core", order);
        var gameplay = new TestModule("gameplay", "Gameplay", order, new ModuleDependency("core", new Version(1, 0, 0)));
        var ui = new TestModule("ui", "UI", order, new ModuleDependency("gameplay", new Version(1, 0, 0)));

        registry.Register(core);
        registry.Register(gameplay);
        registry.Register(ui);

        var loader = new ModuleLoader(registry, eventsHub);

        var loaded = loader.LoadAll();

        Assert.Equal(new[] { "core", "gameplay", "ui" }, order);
        Assert.Equal(3, loaded.Count);
    }

    private sealed class TestModule : IModule
    {
        private readonly List<string> _order;

        public TestModule(string id, string name, List<string> order, params ModuleDependency[] dependencies)
        {
            Id = id;
            Name = name;
            Dependencies = dependencies;
            _order = order;
        }

        public string Id { get; }
        public string Name { get; }
        public Version Version { get; } = new(1, 0, 0);
        public IReadOnlyCollection<ModuleDependency> Dependencies { get; }

        public void Initialize(ModuleContext context)
        {
        }

        public void Start(ModuleContext context)
        {
            _order.Add(Id);
        }
    }
}
