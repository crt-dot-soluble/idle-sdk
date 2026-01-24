namespace IdleSdk.Core.Economy;

public sealed class WalletService
{
    private readonly CurrencyRegistry _registry;
    private readonly Dictionary<Guid, Wallet> _wallets = new();

    public WalletService(CurrencyRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public Wallet GetOrCreateWallet(Guid profileId)
    {
        if (_wallets.TryGetValue(profileId, out var wallet))
        {
            return wallet;
        }

        wallet = new Wallet();
        _wallets[profileId] = wallet;
        return wallet;
    }

    public void Credit(Guid profileId, string currencyId, long amount)
    {
        _registry.Get(currencyId);
        var wallet = GetOrCreateWallet(profileId);
        wallet.Credit(currencyId, amount);
    }

    public void Debit(Guid profileId, string currencyId, long amount)
    {
        _registry.Get(currencyId);
        var wallet = GetOrCreateWallet(profileId);
        wallet.Debit(currencyId, amount);
    }
}
