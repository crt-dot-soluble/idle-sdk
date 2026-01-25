namespace IdleSdk.Core.Actions;

public sealed class ActionRunner
{
    private readonly ActionRegistry _registry;
    private readonly Dictionary<string, ActionExecution> _executions = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<IActionModifier> _modifiers = new();

    public ActionRunner(ActionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public void RegisterModifier(IActionModifier modifier)
    {
        if (modifier is null)
        {
            throw new ArgumentNullException(nameof(modifier));
        }

        _modifiers.Add(modifier);
    }

    public ActionResult Step(string actionId, ActionContext context, TimeSpan delta)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var definition = _registry.GetDefinition(actionId);
        var execution = GetOrCreateExecution(actionId);
        if (!execution.CanExecute(definition, context.StartedAt))
        {
            return new ActionResult(false, "cooldown");
        }

        var handler = _registry.GetHandler(actionId);
        var result = handler.Execute(context, delta);

        foreach (var modifier in _modifiers)
        {
            result = modifier.Apply(context, result, definition);
        }

        if (result.Completed)
        {
            execution.MarkCompleted(context.StartedAt);
        }

        return result;
    }

    private ActionExecution GetOrCreateExecution(string actionId)
    {
        if (_executions.TryGetValue(actionId, out var execution))
        {
            return execution;
        }

        execution = new ActionExecution();
        _executions[actionId] = execution;
        return execution;
    }

    public IReadOnlyDictionary<string, ActionCooldownState> GetCooldownSnapshot()
    {
        return _executions.ToDictionary(pair => pair.Key, pair => pair.Value.GetState(), StringComparer.OrdinalIgnoreCase);
    }

    public void RestoreCooldownSnapshot(IReadOnlyDictionary<string, ActionCooldownState> snapshot)
    {
        if (snapshot is null)
        {
            throw new ArgumentNullException(nameof(snapshot));
        }

        foreach (var pair in snapshot)
        {
            var execution = GetOrCreateExecution(pair.Key);
            execution.Restore(pair.Value);
        }
    }
}
