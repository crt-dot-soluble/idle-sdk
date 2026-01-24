namespace IdleSdk.Core.Actions;

public sealed class ActionRunner
{
    private readonly ActionRegistry _registry;

    public ActionRunner(ActionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public ActionResult Step(string actionId, ActionContext context, TimeSpan delta)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var handler = _registry.GetHandler(actionId);
        return handler.Execute(context, delta);
    }
}
