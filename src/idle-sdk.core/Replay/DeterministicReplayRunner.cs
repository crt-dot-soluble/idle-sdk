namespace IdleSdk.Core.Replay;

public sealed class DeterministicReplayRunner
{
    public IReadOnlyList<string> Run(int ticks, Func<int, string> step)
    {
        if (ticks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(ticks), "Ticks must be positive.");
        }

        if (step is null)
        {
            throw new ArgumentNullException(nameof(step));
        }

        var outputs = new List<string>(ticks);
        for (var i = 0; i < ticks; i++)
        {
            outputs.Add(step(i));
        }

        return outputs;
    }
}
