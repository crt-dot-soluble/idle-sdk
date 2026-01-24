namespace IdleSdk.Core.Economy;

public sealed class Wallet
{
    private readonly Dictionary<string, long> _balances = new(StringComparer.OrdinalIgnoreCase);

    public long GetBalance(string currencyId)
    {
        if (!_balances.TryGetValue(currencyId, out var balance))
        {
            return 0;
        }

        return balance;
    }

    public void Credit(string currencyId, long amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        }

        _balances[currencyId] = GetBalance(currencyId) + amount;
    }

    public void Debit(string currencyId, long amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be non-negative.");
        }

        var balance = GetBalance(currencyId);
        if (balance < amount)
        {
            throw new InvalidOperationException($"Insufficient balance for '{currencyId}'.");
        }

        _balances[currencyId] = balance - amount;
    }

    public IReadOnlyDictionary<string, long> Balances => _balances;
}
