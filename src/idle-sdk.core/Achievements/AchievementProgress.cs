namespace IdleSdk.Core.Achievements;

public sealed class AchievementProgress
{
    public AchievementProgress(AchievementDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        CurrentValue = 0;
        Completed = false;
    }

    public AchievementDefinition Definition { get; }
    public int CurrentValue { get; private set; }
    public bool Completed { get; private set; }

    public void AddProgress(int amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Progress amount must be positive.");
        }

        if (Completed)
        {
            return;
        }

        CurrentValue = Math.Min(Definition.TargetValue, CurrentValue + amount);
        if (CurrentValue >= Definition.TargetValue)
        {
            Completed = true;
        }
    }
}
