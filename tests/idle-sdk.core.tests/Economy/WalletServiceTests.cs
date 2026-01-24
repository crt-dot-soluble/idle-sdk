using IdleSdk.Core.Economy;

namespace IdleSdk.Core.Tests.Economy;

public class WalletServiceTests
{
    [Fact]
    public void WalletService_Credits_And_Debits()
    {
        var registry = new CurrencyRegistry();
        registry.Register(new CurrencyDefinition("gold", "Gold", false));

        var service = new WalletService(registry);
        var profileId = Guid.NewGuid();

        service.Credit(profileId, "gold", 100);
        service.Debit(profileId, "gold", 40);

        var wallet = service.GetOrCreateWallet(profileId);
        Assert.Equal(60, wallet.GetBalance("gold"));
    }

    [Fact]
    public void Wallet_Debit_Throws_When_Insufficient()
    {
        var wallet = new Wallet();

        Assert.Throws<InvalidOperationException>(() => wallet.Debit("gold", 1));
    }
}
