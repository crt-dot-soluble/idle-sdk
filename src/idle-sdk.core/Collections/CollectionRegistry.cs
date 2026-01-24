namespace IdleSdk.Core.Collections;

public sealed class CollectionRegistry
{
    private readonly Dictionary<string, CollectionDefinition> _definitions = new(StringComparer.OrdinalIgnoreCase);

    public void Register(CollectionDefinition definition)
    {
        if (definition is null)
        {
            throw new ArgumentNullException(nameof(definition));
        }

        if (_definitions.ContainsKey(definition.Id))
        {
            throw new InvalidOperationException($"Collection '{definition.Id}' is already registered.");
        }

        _definitions[definition.Id] = definition;
    }

    public CollectionDefinition Get(string collectionId)
    {
        if (!_definitions.TryGetValue(collectionId, out var definition))
        {
            throw new KeyNotFoundException($"Collection '{collectionId}' was not found.");
        }

        return definition;
    }
}
