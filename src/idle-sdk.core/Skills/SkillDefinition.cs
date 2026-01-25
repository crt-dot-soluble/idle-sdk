namespace IdleSdk.Core.Skills;

public sealed record SkillDefinition(
    string Id,
    string Name,
    int MaxLevel,
    IReadOnlyDictionary<int, string> Unlocks
);
