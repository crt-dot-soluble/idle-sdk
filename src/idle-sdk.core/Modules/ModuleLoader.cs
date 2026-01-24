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
            module.Initialize(_context);
        }

        return ordered;
    }
}
