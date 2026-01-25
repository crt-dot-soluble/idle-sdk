namespace IdleSdk.Core.Combat;

public sealed record CombatTickResult(IReadOnlyList<CombatLogEntry> LogEntries, IReadOnlyList<CombatAiDecision> AiDecisions);
