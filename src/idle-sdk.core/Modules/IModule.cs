namespace IdleSdk.Core.Modules;

public interface IModule
{
    string Name { get; }
    Version Version { get; }
    IReadOnlyCollection<ModuleDependency> Dependencies { get; }
    void Initialize(ModuleContext context);
}
