namespace IdleSdk.Core.Actions;

public sealed class ActionRegistry
{
    private readonly Dictionary<string, ActionDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IActionHandler> _handlers = new(StringComparer.OrdinalIgnoreCase);

    public void RegisterDefinition(ActionDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Action definition '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public void RegisterHandler(IActionHandler handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        if (_handlers.ContainsKey(handler.ActionId))
        {
            throw new InvalidOperationException($"Action handler '{handler.ActionId}' is already registered.");
        }

        _handlers[handler.ActionId] = handler;
    }

    public ActionDefinition GetDefinition(string actionId)
    {
        if (!_definitions.TryGetValue(actionId, out var definition))
        {
            throw new KeyNotFoundException($"Action definition '{actionId}' was not found.");
        }

        return definition;
    }

    public IActionHandler GetHandler(string actionId)
    {
        if (!_handlers.TryGetValue(actionId, out var handler))
        {
            throw new KeyNotFoundException($"Action handler '{actionId}' was not found.");
        }

        return handler;
    }

    public IReadOnlyCollection<ActionDefinition> Definitions => _definitions.Values.ToList();
}
