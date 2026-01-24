namespace IdleSdk.Core.Economy;

public sealed class CurrencyRegistry
{
    private readonly Dictionary<string, CurrencyDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(CurrencyDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Currency '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public CurrencyDefinition Get(string currencyId)
    {
        if (!_definitions.TryGetValue(currencyId, out var definition))
        {
            throw new KeyNotFoundException($"Currency '{currencyId}' was not found.");
        }

        return definition;
    }

    public IReadOnlyCollection<CurrencyDefinition> Definitions => _definitions.Values.ToList();
}
