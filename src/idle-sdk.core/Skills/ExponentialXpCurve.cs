namespace IdleSdk.Core.Skills;

public sealed class ExponentialXpCurve : IXpCurve
{
    private readonly long _baseXp;
    private readonly double _growthFactor;

    public ExponentialXpCurve(long baseXp, double growthFactor)
    {
        if (baseXp < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseXp), "XP values must be non-negative.");
        }

        if (growthFactor < 1d)
        {
            throw new ArgumentOutOfRangeException(nameof(growthFactor), "Growth factor must be >= 1.");
        }

        _baseXp = baseXp;
        _growthFactor = growthFactor;
    }

    public long GetTotalXpForLevel(int level)
    {
        if (level < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(level), "Level must be >= 1.");
        }

        if (level == 1)
        {
            return 0;
        }

        double total = 0;
        for (var current = 2; current <= level; current++)
        {
            var segment = _baseXp * Math.Pow(_growthFactor, current - 2);
            total += segment;
        }

        return (long)Math.Round(total, MidpointRounding.AwayFromZero);
    }
}
