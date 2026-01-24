namespace IdleSdk.Core.Collections;

public sealed class CollectionProgress
{
    private readonly HashSet<string> _collected = new(StringComparer.OrdinalIgnoreCase);

    public CollectionProgress(CollectionDefinition definition)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public CollectionDefinition Definition { get; }
    public IReadOnlyCollection<string> Collected => _collected;

    public void AddItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item id must be provided.", nameof(itemId));
        }

        _collected.Add(itemId);
    }

    public bool IsComplete => Definition.ItemIds.All(id => _collected.Contains(id));
}
