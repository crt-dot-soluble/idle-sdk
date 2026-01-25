namespace IdleSdk.Core.Skills;

public sealed class SkillProgress
{
    private readonly List<string> _unlocks = new();

    public SkillProgress(SkillDefinition definition, IXpCurve xpCurve)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        XpCurve = xpCurve ?? throw new ArgumentNullException(nameof(xpCurve));
        Level = 1;
        TotalXp = 0;
        ApplyUnlocks(1, 1);
    }

    public SkillDefinition Definition { get; }
    public IXpCurve XpCurve { get; }
    public int Level { get; private set; }
    public long TotalXp { get; private set; }
    public IReadOnlyCollection<string> Unlocks => _unlocks.AsReadOnly();

    public void AddXp(long amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "XP amount must be non-negative.");
        }

        TotalXp += amount;
        RecalculateLevel();
    }

    private void RecalculateLevel()
    {
        var maxLevel = Definition.MaxLevel;
        var previousLevel = Level;
        var newLevel = Level;

        while (newLevel < maxLevel && TotalXp >= XpCurve.GetTotalXpForLevel(newLevel + 1))
        {
            newLevel++;
        }

        Level = newLevel;
        if (newLevel > previousLevel)
        {
            ApplyUnlocks(previousLevel + 1, newLevel);
        }
    }

    private void ApplyUnlocks(int fromLevel, int toLevel)
    {
        if (Definition.Unlocks is null || Definition.Unlocks.Count == 0)
        {
            return;
        }

        for (var level = fromLevel; level <= toLevel; level++)
        {
            if (Definition.Unlocks.TryGetValue(level, out var unlock))
            {
                _unlocks.Add(unlock);
            }
        }
    }
}
