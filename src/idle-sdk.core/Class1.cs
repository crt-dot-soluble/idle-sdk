namespace IdleSdk.Core;

public sealed class SimulationClock
{
	private TimeSpan _accumulator = TimeSpan.Zero;

	public SimulationClock(int tickRate)
	{
		if (tickRate <= 0)
		{
			throw new ArgumentOutOfRangeException(nameof(tickRate), "Tick rate must be positive.");
		}

		TickRate = tickRate;
		TickDuration = TimeSpan.FromSeconds(1d / tickRate);
	}

	public int TickRate { get; }
	public TimeSpan TickDuration { get; }
	public long TotalTicks { get; private set; }
	public TimeSpan TotalSimulatedTime => TimeSpan.FromTicks(TotalTicks * TickDuration.Ticks);

	public int Advance(TimeSpan realDelta)
	{
		if (realDelta < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException(nameof(realDelta), "Delta must be non-negative.");
		}

		_accumulator += realDelta;
		var ticks = 0;

		while (_accumulator >= TickDuration)
		{
			_accumulator -= TickDuration;
			TotalTicks++;
			ticks++;
		}

		return ticks;
	}

	public void Reset()
	{
		_accumulator = TimeSpan.Zero;
		TotalTicks = 0;
	}
}
