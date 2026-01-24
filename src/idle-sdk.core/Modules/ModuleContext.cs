using IdleSdk.Core.Events;

namespace IdleSdk.Core.Modules;

public sealed class ModuleContext
{
    public ModuleContext(EventHub events)
    {
        Events = events ?? throw new ArgumentNullException(nameof(events));
    }

    public EventHub Events { get; }
}
