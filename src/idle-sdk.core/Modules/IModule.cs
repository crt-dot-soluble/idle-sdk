namespace IdleSdk.Core.Modules;

public interface IModule
{
    string Id { get; }
    string Name { get; }
    Version Version { get; }
    IReadOnlyCollection<ModuleDependency> Dependencies { get; }
    IReadOnlyCollection<IModule> Submodules => Array.Empty<IModule>();
    void Initialize(ModuleContext context);
    void Start(ModuleContext context) { }
    void Stop(ModuleContext context) { }
    void Reload(ModuleContext context) { }
}
