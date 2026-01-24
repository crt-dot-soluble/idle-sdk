namespace IdleSdk.Core.Collections;

public sealed class CollectionService
{
    private readonly CollectionRegistry _registry;
    private readonly Dictionary<Guid, Dictionary<string, CollectionProgress>> _progress = new();

    public CollectionService(CollectionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public CollectionProgress GetOrCreate(Guid profileId, string collectionId)
    {
        if (!_progress.TryGetValue(profileId, out var profileCollections))
        {
            profileCollections = new Dictionary<string, CollectionProgress>(StringComparer.OrdinalIgnoreCase);
            _progress[profileId] = profileCollections;
        }

        if (profileCollections.TryGetValue(collectionId, out var progress))
        {
            return progress;
        }

        var definition = _registry.Get(collectionId);
        progress = new CollectionProgress(definition);
        profileCollections[collectionId] = progress;
        return progress;
    }

    public void AddItem(Guid profileId, string collectionId, string itemId)
    {
        var progress = GetOrCreate(profileId, collectionId);
        progress.AddItem(itemId);
    }
}
