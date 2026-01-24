using IdleSdk.Core;

namespace IdleSdk.Core.Timing;

public sealed class TickScheduler
{
    private readonly SimulationClock _clock;

    public TickScheduler(SimulationClock clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public event EventHandler<TickContext>? TickStart;
    public event EventHandler<TickContext>? TickEnd;

    public int Step(TimeSpan realDelta, Action<TickContext> tickAction)
    {
        if (tickAction is null)
        {
            throw new ArgumentNullException(nameof(tickAction));
        }

        var ticks = _clock.Advance(realDelta);

        for (var i = 0; i < ticks; i++)
        {
            var context = new TickContext(_clock.TotalTicks - ticks + i + 1, _clock.TotalSimulatedTime - TimeSpan.FromTicks(_clock.TickDuration.Ticks * (ticks - i - 1)));
            TickStart?.Invoke(this, context);
            tickAction(context);
            TickEnd?.Invoke(this, context);
        }

        return ticks;
    }
}
