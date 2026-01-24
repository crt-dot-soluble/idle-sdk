namespace IdleSdk.Core.Sandbox;

public sealed class SandboxConsole
{
    private readonly Dictionary<string, Func<SandboxCommand, SandboxResult>> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public bool Enabled { get; private set; }

    public void Enable() => Enabled = true;
    public void Disable() => Enabled = false;

    public void Register(string commandName, Func<SandboxCommand, SandboxResult> handler)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new ArgumentException("Command name must be provided.", nameof(commandName));
        }

        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (_handlers.ContainsKey(commandName))
        {
            throw new InvalidOperationException($"Sandbox command '{commandName}' is already registered.");
        }

        _handlers[commandName] = handler;
    }

    public SandboxResult Execute(SandboxCommand command)
    {
        if (!Enabled)
        {
            return new SandboxResult(false, "Sandbox is disabled.");
        }

        if (!_handlers.TryGetValue(command.Name, out var handler))
        {
            return new SandboxResult(false, $"Unknown command '{command.Name}'.");
        }

        return handler(command);
    }
}
