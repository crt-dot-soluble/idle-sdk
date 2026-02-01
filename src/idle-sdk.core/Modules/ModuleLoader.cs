using IdleSdk.Core.Events;

namespace IdleSdk.Core.Modules;

public sealed class ModuleLoader
{
    private readonly ModuleRegistry _registry;
    private readonly ModuleContext _context;

    public ModuleLoader(ModuleRegistry registry, EventHub events)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _context = new ModuleContext(events ?? throw new ArgumentNullException(nameof(events)));
    }

    public IReadOnlyList<IModule> LoadAll()
    {
        var ordered = _registry.ResolveLoadOrder();

        foreach (var module in ordered)
        {
            if (!_registry.IsInitialized(module.Id))
            {
                module.Initialize(_context);
                _registry.MarkInitialized(module.Id);
            }
        }

        StartEnabledModules();

        return ordered;
    }

    public void Enable(string moduleId)
    {
        _registry.Enable(moduleId);
        StartEnabledModules();
    }

    public void Disable(string moduleId)
    {
        if (!_registry.TryGetEntry(moduleId, out var module, out _))
        {
            throw new ModuleRegistrationException($"Module '{moduleId}' is not registered.");
        }

        if (_registry.IsStarted(module.Id))
        {
            module.Stop(_context);
            _registry.MarkStarted(module.Id, false);
        }

        _registry.Disable(moduleId);
    }

    public void Reload(string moduleId)
    {
        if (!_registry.TryGetEntry(moduleId, out var module, out _))
        {
            throw new ModuleRegistrationException($"Module '{moduleId}' is not registered.");
        }

        module.Reload(_context);
    }

    private void StartEnabledModules()
    {
        var ordered = _registry.ResolveLoadOrder(includeDisabled: false);
        foreach (var module in ordered)
        {
            if (_registry.IsStarted(module.Id))
            {
                continue;
            }

            if (!_registry.IsInitialized(module.Id))
            {
                module.Initialize(_context);
                _registry.MarkInitialized(module.Id);
            }

            module.Start(_context);
            _registry.MarkStarted(module.Id, true);
        }
    }
}
