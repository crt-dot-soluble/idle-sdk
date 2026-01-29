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
        if (ticks == 0)
        {
            return 0;
        }

        var tickDurationTicks = _clock.TickDuration.Ticks;
        var firstTickIndex = _clock.TotalTicks - ticks + 1;
        var firstSimulatedTicks = _clock.TotalSimulatedTime.Ticks - tickDurationTicks * (ticks - 1);

        for (var i = 0; i < ticks; i++)
        {
            var context = new TickContext(firstTickIndex + i, TimeSpan.FromTicks(firstSimulatedTicks + tickDurationTicks * i));
            TickStart?.Invoke(this, context);
            tickAction(context);
            TickEnd?.Invoke(this, context);
        }

        return ticks;
    }
}
