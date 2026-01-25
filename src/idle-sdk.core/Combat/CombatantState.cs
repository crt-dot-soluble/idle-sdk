namespace IdleSdk.Core.Combat;

public sealed class CombatantState
{
    public CombatantState(string id, CombatantStats stats)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Stats = stats ?? throw new ArgumentNullException(nameof(stats));
        CurrentHealth = stats.MaxHealth;
    }

    public string Id { get; }
    public CombatantStats Stats { get; }
    public int CurrentHealth { get; private set; }
    public CombatantEffects Effects { get; } = new();

    public bool IsDefeated => CurrentHealth <= 0;

    public void ApplyDamage(int amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Damage must be non-negative.");
        }

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
    }
}
