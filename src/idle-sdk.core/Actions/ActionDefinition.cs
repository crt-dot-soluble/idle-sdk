namespace IdleSdk.Core.Actions;

public sealed record ActionDefinition(
    string Id,
    string Name,
    TimeSpan Duration
);
