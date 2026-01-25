namespace IdleSdk.Core.Combat;

public sealed record CombatAiDecision(string AttackerId, string TargetId, long? Seed);
