namespace IdleSdk.Core.Skills;

public sealed class LinearXpCurve : IXpCurve
{
    private readonly long _baseXp;
    private readonly long _incrementPerLevel;

    public LinearXpCurve(long baseXp, long incrementPerLevel)
    {
        if (baseXp < 0 || incrementPerLevel < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(baseXp), "XP values must be non-negative.");
        }

        _baseXp = baseXp;
        _incrementPerLevel = incrementPerLevel;
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

        return _baseXp + (level - 2) * _incrementPerLevel;
    }
}
