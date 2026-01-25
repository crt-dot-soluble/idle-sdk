namespace IdleSdk.Core.Actions;

public sealed class ActionExecution
{
	private DateTimeOffset? _lastCompletedAt;

	public bool CanExecute(ActionDefinition definition, DateTimeOffset now)
	{
		if (definition is null)
		{
			throw new ArgumentNullException(nameof(definition));
		}

		if (definition.Cooldown <= TimeSpan.Zero)
		{
			return true;
		}

		if (_lastCompletedAt is null)
		{
			return true;
		}

		return now - _lastCompletedAt.Value >= definition.Cooldown;
	}

	public void MarkCompleted(DateTimeOffset now)
	{
		_lastCompletedAt = now;
	}

	public ActionCooldownState GetState() => new(_lastCompletedAt);

	public void Restore(ActionCooldownState state)
	{
		if (state is null)
		{
			throw new ArgumentNullException(nameof(state));
		}

		_lastCompletedAt = state.LastCompletedAt;
	}
}
