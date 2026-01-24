namespace IdleSdk.Core.Actions;

public interface IActionHandler
{
    string ActionId { get; }
    ActionResult Execute(ActionContext context, TimeSpan delta);
}
