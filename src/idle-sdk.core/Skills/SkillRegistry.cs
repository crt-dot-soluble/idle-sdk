namespace IdleSdk.Core.Skills;

public sealed class SkillRegistry
{
    private readonly Dictionary<string, SkillDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(SkillDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Skill '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public SkillDefinition Get(string skillId)
    {
        if (!_definitions.TryGetValue(skillId, out var definition))
        {
            throw new KeyNotFoundException($"Skill '{skillId}' was not found.");
        }

        return definition;
    }

    public IReadOnlyCollection<SkillDefinition> Definitions => _definitions.Values.ToList();
}
