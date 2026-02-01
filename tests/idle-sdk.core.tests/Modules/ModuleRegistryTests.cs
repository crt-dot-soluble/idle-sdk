using IdleSdk.Core.Modules;

namespace IdleSdk.Core.Tests.Modules;

public class ModuleRegistryTests
{
    [Fact]
    public void Register_Rejects_Empty_Name()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("core", "");

        var ex = Assert.Throws<ModuleRegistrationException>(() => registry.Register(module));
        Assert.Contains("name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Register_Rejects_Empty_Id()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("", "Core");

        var ex = Assert.Throws<ModuleRegistrationException>(() => registry.Register(module));
        Assert.Contains("id", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ResolveLoadOrder_Rejects_Invalid_Dependency_Name()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("alpha", "Alpha", new[] { new ModuleDependency(" ", new Version(1, 0, 0)) });
        registry.Register(module);

        Assert.Throws<ModuleDependencyException>(() => registry.ResolveLoadOrder());
    }

    [Fact]
    public void ResolveLoadOrder_Throws_On_Missing_Dependency()
    {
        var registry = new ModuleRegistry();
        registry.Register(new TestModule("alpha", "Alpha", new[] { new ModuleDependency("beta", new Version(1, 0, 0)) }));

        Assert.Throws<ModuleDependencyException>(() => registry.ResolveLoadOrder());
    }

    [Fact]
    public void RegisterRange_Rejects_Null_List()
    {
        var registry = new ModuleRegistry();

        Assert.Throws<ArgumentNullException>(() => registry.RegisterRange(null!));
    }

    [Fact]
    public void Register_Registers_Submodules()
    {
        var registry = new ModuleRegistry();
        var child = new TestModule("child", "Child");
        var parent = new TestModule("parent", "Parent", submodules: new[] { child });

        registry.Register(parent);

        Assert.Equal(2, registry.Modules.Count);
        Assert.Contains(registry.Registrations, entry => entry.Id == "child" && entry.ParentId == "parent");
    }

    [Fact]
    public void Remove_Removes_Submodules()
    {
        var registry = new ModuleRegistry();
        var child = new TestModule("child", "Child");
        var parent = new TestModule("parent", "Parent", submodules: new[] { child });

        registry.Register(parent);
        registry.Remove("parent");

        Assert.Empty(registry.Modules);
    }

    [Fact]
    public void Replace_Preserves_Enabled_State()
    {
        var registry = new ModuleRegistry();
        var module = new TestModule("alpha", "Alpha");
        registry.Register(module);
        registry.Disable("alpha");

        registry.Replace(new TestModule("alpha", "Alpha v2"));

        Assert.False(registry.IsEnabled("alpha"));
        Assert.Contains(registry.Registrations, entry => entry.Id == "alpha" && entry.Name == "Alpha v2");
    }

    private sealed class TestModule : IModule
    {
        public TestModule(string id, string name, ModuleDependency[]? dependencies = null, IReadOnlyCollection<IModule>? submodules = null)
        {
            Id = id;
            Name = name;
            Dependencies = dependencies ?? Array.Empty<ModuleDependency>();
            Submodules = submodules ?? Array.Empty<IModule>();
        }

        public string Id { get; }
        public string Name { get; }
        public Version Version { get; } = new(1, 0, 0);
        public IReadOnlyCollection<ModuleDependency> Dependencies { get; }
        public IReadOnlyCollection<IModule> Submodules { get; }

        public void Initialize(ModuleContext context)
        {
        }
    }
}
