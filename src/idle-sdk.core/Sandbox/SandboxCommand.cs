namespace IdleSdk.Core.Sandbox;

public sealed record SandboxCommand(string Name, IReadOnlyDictionary<string, string> Arguments);
