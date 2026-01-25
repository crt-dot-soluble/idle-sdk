namespace IdleSdk.Core.Persistence;

public sealed class SnapshotService<T>
{
    private readonly ISnapshotStore _store;
    private readonly IStateSerializer<T> _serializer;

    public SnapshotService(ISnapshotStore store, IStateSerializer<T> serializer)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public async Task SaveAsync(string profileId, T state, string schemaVersion, CancellationToken cancellationToken = default)
    {
        var payload = _serializer.Serialize(state);
        var snapshot = new Snapshot(new SnapshotMetadata(Guid.NewGuid(), profileId, schemaVersion, DateTimeOffset.UtcNow), payload);
        await _store.SaveSnapshotAsync(snapshot, cancellationToken);
    }

    public async Task<T?> LoadLatestAsync(string profileId, CancellationToken cancellationToken = default)
    {
        var snapshot = await _store.GetLatestSnapshotAsync(profileId, cancellationToken);
        return snapshot is null ? default : _serializer.Deserialize(snapshot.Payload);
    }
}
