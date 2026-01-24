namespace IdleSdk.Core.Collections;

public sealed record CollectionDefinition(string Id, string Name, IReadOnlyList<string> ItemIds);
