namespace IdleSdk.Core.Actions;

public interface IActionModifier
{
	ActionResult Apply(ActionContext context, ActionResult result, ActionDefinition definition);
}
