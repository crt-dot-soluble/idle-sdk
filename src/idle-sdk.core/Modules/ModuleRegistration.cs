namespace IdleSdk.Core.Modules;

public sealed record ModuleRegistration(
    string Id,
    string Name,
    Version Version,
    string? ParentId,
    bool Enabled,
    IReadOnlyCollection<string> Submodules,
    IReadOnlyCollection<ModuleDependency> Dependencies);
