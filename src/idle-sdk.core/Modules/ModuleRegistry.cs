namespace IdleSdk.Core.Modules;

public sealed class ModuleRegistry
{
    private readonly Dictionary<string, IModule> _modules = new(StringComparer.OrdinalIgnoreCase);

    public void Register(IModule module)
    {
        if (module is null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        if (_modules.ContainsKey(module.Name))
        {
            throw new ModuleRegistrationException($"Module '{module.Name}' is already registered.");
        }

        _modules[module.Name] = module;
    }

    public void RegisterRange(IEnumerable<IModule> modules)
    {
        if (modules is null)
        {
            throw new ArgumentNullException(nameof(modules));
        }

        foreach (var module in modules)
        {
            Register(module);
        }
    }

    public IReadOnlyCollection<IModule> Modules => _modules.Values.ToList();

    public IReadOnlyList<IModule> ResolveLoadOrder()
    {
        var resolved = new List<IModule>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in _modules.Values)
        {
            Visit(module, resolved, visiting, visited);
        }

        return resolved;
    }

    private void Visit(IModule module, ICollection<IModule> resolved, ISet<string> visiting, ISet<string> visited)
    {
        if (visited.Contains(module.Name))
        {
            return;
        }

        if (!visiting.Add(module.Name))
        {
            throw new ModuleDependencyException($"Cyclic dependency detected at '{module.Name}'.");
        }

        foreach (var dependency in module.Dependencies)
        {
            if (!_modules.TryGetValue(dependency.Name, out var dependencyModule))
            {
                throw new ModuleDependencyException($"Missing dependency '{dependency.Name}' for module '{module.Name}'.");
            }

            if (dependencyModule.Version < dependency.MinimumVersion)
            {
                throw new ModuleVersionException($"Module '{module.Name}' requires '{dependency.Name}' >= {dependency.MinimumVersion}.");
            }

            Visit(dependencyModule, resolved, visiting, visited);
        }

        visiting.Remove(module.Name);
        visited.Add(module.Name);
        resolved.Add(module);
    }
}
