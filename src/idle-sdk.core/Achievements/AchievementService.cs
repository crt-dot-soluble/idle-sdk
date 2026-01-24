namespace IdleSdk.Core.Achievements;

public sealed class AchievementService
{
    private readonly AchievementRegistry _registry;
    private readonly Dictionary<Guid, Dictionary<string, AchievementProgress>> _progress = new();

    public AchievementService(AchievementRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public AchievementProgress GetOrCreate(Guid profileId, string achievementId)
    {
        if (!_progress.TryGetValue(profileId, out var profileAchievements))
        {
            profileAchievements = new Dictionary<string, AchievementProgress>(StringComparer.OrdinalIgnoreCase);
            _progress[profileId] = profileAchievements;
        }

        if (profileAchievements.TryGetValue(achievementId, out var progress))
        {
            return progress;
        }

        var definition = _registry.Get(achievementId);
        progress = new AchievementProgress(definition);
        profileAchievements[achievementId] = progress;
        return progress;
    }

    public void AddProgress(Guid profileId, string achievementId, int amount)
    {
        var progress = GetOrCreate(profileId, achievementId);
        progress.AddProgress(amount);
    }
}
