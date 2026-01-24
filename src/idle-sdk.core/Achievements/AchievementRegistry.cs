namespace IdleSdk.Core.Achievements;

public sealed class AchievementRegistry
{
    private readonly Dictionary<string, AchievementDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(AchievementDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Achievement '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public AchievementDefinition Get(string achievementId)
    {
        if (!_definitions.TryGetValue(achievementId, out var definition))
        {
            throw new KeyNotFoundException($"Achievement '{achievementId}' was not found.");
        }

        return definition;
    }
}
