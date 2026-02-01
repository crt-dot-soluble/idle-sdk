namespace IdleSdk.Core.Modules;

public sealed class ModuleRegistry
{
    private readonly Dictionary<string, ModuleEntry> _modules = new(StringComparer.OrdinalIgnoreCase);

    private sealed class ModuleEntry
    {
        public ModuleEntry(IModule module, string? parentId, bool enabled)
        {
            Module = module;
            ParentId = parentId;
            Enabled = enabled;
        }

        public IModule Module { get; }
        public string? ParentId { get; }
        public List<string> SubmoduleIds { get; } = new();
        public bool Enabled { get; set; }
        public bool Initialized { get; set; }
        public bool Started { get; set; }
    }

    public void Register(IModule module)
    {
        RegisterInternal(module, null);
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

    public void Replace(IModule module)
    {
        if (module is null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        var parentId = _modules.TryGetValue(module.Id, out var existing) ? existing.ParentId : null;
        var enabled = existing?.Enabled ?? true;
        RemoveInternal(module.Id, cascadeSubmodules: true);
        RegisterInternal(module, parentId);
        _modules[module.Id].Enabled = enabled;
    }

    public void Remove(string moduleId)
    {
        RemoveInternal(moduleId, cascadeSubmodules: true);
    }

    public IReadOnlyCollection<IModule> Modules => _modules.Values.Select(entry => entry.Module).ToList();

    public IReadOnlyCollection<ModuleRegistration> Registrations => _modules.Values
        .Select(entry => new ModuleRegistration(
            entry.Module.Id,
            entry.Module.Name,
            entry.Module.Version,
            entry.ParentId,
            entry.Enabled,
            entry.SubmoduleIds.ToList(),
            entry.Module.Dependencies))
        .ToList();

    public bool IsEnabled(string moduleId)
    {
        if (!_modules.TryGetValue(moduleId, out var entry))
        {
            throw new ModuleRegistrationException($"Module '{moduleId}' is not registered.");
        }

        return entry.Enabled;
    }

    public void Enable(string moduleId)
    {
        if (!_modules.TryGetValue(moduleId, out var entry))
        {
            throw new ModuleRegistrationException($"Module '{moduleId}' is not registered.");
        }

        entry.Enabled = true;
    }

    public void Disable(string moduleId)
    {
        if (!_modules.TryGetValue(moduleId, out var entry))
        {
            throw new ModuleRegistrationException($"Module '{moduleId}' is not registered.");
        }

        entry.Enabled = false;
    }

    public IReadOnlyList<IModule> ResolveLoadOrder(bool includeDisabled = true)
    {
        var resolved = new List<IModule>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var module in _modules.Values)
        {
            if (!includeDisabled && !module.Enabled)
            {
                continue;
            }
            Visit(module, resolved, visiting, visited, includeDisabled);
        }

        return resolved;
    }

    internal bool TryGetEntry(string moduleId, out IModule module, out bool enabled)
    {
        if (_modules.TryGetValue(moduleId, out var entry))
        {
            module = entry.Module;
            enabled = entry.Enabled;
            return true;
        }

        module = null!;
        enabled = false;
        return false;
    }

    internal void MarkInitialized(string moduleId)
    {
        if (_modules.TryGetValue(moduleId, out var entry))
        {
            entry.Initialized = true;
        }
    }

    internal bool IsInitialized(string moduleId)
    {
        return _modules.TryGetValue(moduleId, out var entry) && entry.Initialized;
    }

    internal void MarkStarted(string moduleId, bool started)
    {
        if (_modules.TryGetValue(moduleId, out var entry))
        {
            entry.Started = started;
        }
    }

    internal bool IsStarted(string moduleId)
    {
        return _modules.TryGetValue(moduleId, out var entry) && entry.Started;
    }

    private void RegisterInternal(IModule module, string? parentId)
    {
        if (module is null)
        {
            throw new ArgumentNullException(nameof(module));
        }

        if (string.IsNullOrWhiteSpace(module.Id))
        {
            throw new ModuleRegistrationException("Module id must be provided.");
        }

        if (string.IsNullOrWhiteSpace(module.Name))
        {
            throw new ModuleRegistrationException("Module name must be provided.");
        }

        if (module.Dependencies is null)
        {
            throw new ModuleRegistrationException($"Module '{module.Name}' must define dependencies.");
        }

        if (_modules.ContainsKey(module.Id))
        {
            throw new ModuleRegistrationException($"Module '{module.Id}' is already registered.");
        }

        var entry = new ModuleEntry(module, parentId, true);
        _modules[module.Id] = entry;

        if (!string.IsNullOrWhiteSpace(parentId) && _modules.TryGetValue(parentId, out var parent))
        {
            parent.SubmoduleIds.Add(module.Id);
        }

        foreach (var submodule in module.Submodules ?? Array.Empty<IModule>())
        {
            RegisterInternal(submodule, module.Id);
        }
    }

    private void RemoveInternal(string moduleId, bool cascadeSubmodules)
    {
        if (!_modules.TryGetValue(moduleId, out var entry))
        {
            return;
        }

        if (cascadeSubmodules)
        {
            foreach (var submoduleId in entry.SubmoduleIds.ToList())
            {
                RemoveInternal(submoduleId, cascadeSubmodules: true);
            }
        }

        if (!string.IsNullOrWhiteSpace(entry.ParentId)
            && _modules.TryGetValue(entry.ParentId, out var parent))
        {
            parent.SubmoduleIds.Remove(moduleId);
        }

        _modules.Remove(moduleId);
    }

    private void Visit(ModuleEntry entry, ICollection<IModule> resolved, ISet<string> visiting, ISet<string> visited, bool includeDisabled)
    {
        if (visited.Contains(entry.Module.Id))
        {
            return;
        }

        if (!visiting.Add(entry.Module.Id))
        {
            throw new ModuleDependencyException($"Cyclic dependency detected at '{entry.Module.Id}'.");
        }

        foreach (var dependency in entry.Module.Dependencies)
        {
            if (string.IsNullOrWhiteSpace(dependency.Id))
            {
                throw new ModuleDependencyException($"Module '{entry.Module.Id}' has an invalid dependency id.");
            }

            if (!_modules.TryGetValue(dependency.Id, out var dependencyModule))
            {
                throw new ModuleDependencyException($"Missing dependency '{dependency.Id}' for module '{entry.Module.Id}'.");
            }

            if (dependencyModule.Module.Version < dependency.MinimumVersion)
            {
                throw new ModuleVersionException($"Module '{entry.Module.Id}' requires '{dependency.Id}' >= {dependency.MinimumVersion}.");
            }

            if (!includeDisabled && !dependencyModule.Enabled)
            {
                throw new ModuleDependencyException($"Module '{entry.Module.Id}' requires '{dependency.Id}' to be enabled.");
            }

            Visit(dependencyModule, resolved, visiting, visited, includeDisabled);
        }

        visiting.Remove(entry.Module.Id);
        visited.Add(entry.Module.Id);
        resolved.Add(entry.Module);
    }
}
