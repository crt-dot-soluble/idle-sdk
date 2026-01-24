namespace IdleSdk.Core.Skills;

public sealed class SkillProgress
{
    public SkillProgress(SkillDefinition definition, IXpCurve xpCurve)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        XpCurve = xpCurve ?? throw new ArgumentNullException(nameof(xpCurve));
        Level = 1;
        TotalXp = 0;
    }

    public SkillDefinition Definition { get; }
    public IXpCurve XpCurve { get; }
    public int Level { get; private set; }
    public long TotalXp { get; private set; }

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
        var newLevel = Level;

        while (newLevel < maxLevel && TotalXp >= XpCurve.GetTotalXpForLevel(newLevel + 1))
        {
            newLevel++;
        }

        Level = newLevel;
    }
}
