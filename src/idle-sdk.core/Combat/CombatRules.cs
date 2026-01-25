namespace IdleSdk.Core.Combat;

public sealed record CombatRules(int MinDamage, int MaxDamage, int AttackBonus, int DefenseBonus, EffectStackingMode EffectStacking)
{
    public static CombatRules Default { get; } = new(0, int.MaxValue, 0, 0, EffectStackingMode.Additive);
}
