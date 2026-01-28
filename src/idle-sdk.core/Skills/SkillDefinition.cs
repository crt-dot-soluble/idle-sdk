namespace IdleSdk.Core.Skills;

public sealed record SkillDefinition(
    string Id,
    string Name,
    string Icon,
    int MaxLevel,
    IReadOnlyDictionary<int, string> Unlocks
);
