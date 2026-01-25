namespace IdleSdk.Core.Data;

public sealed class ContentPackRegistry
{
    private readonly Dictionary<string, ContentPackState> _packs = new(StringComparer.OrdinalIgnoreCase);

    public void Register(ContentPackState pack)
    {
        if (pack is null)
        {
            throw new ArgumentNullException(nameof(pack));
        }

        if (_packs.ContainsKey(pack.Manifest.Id))
        {
            throw new InvalidOperationException($"Content pack '{pack.Manifest.Id}' is already registered.");
        }

        _packs[pack.Manifest.Id] = pack;
    }

    public void Replace(ContentPackState pack)
    {
        if (pack is null)
        {
            throw new ArgumentNullException(nameof(pack));
        }

        _packs[pack.Manifest.Id] = pack;
    }

    public bool Remove(string packId)
    {
        return _packs.Remove(packId);
    }

    public ContentPackState Get(string packId)
    {
        if (!_packs.TryGetValue(packId, out var pack))
        {
            throw new KeyNotFoundException($"Content pack '{packId}' was not found.");
        }

        return pack;
    }

    public IReadOnlyCollection<ContentPackState> Packs => _packs.Values.ToList();

    public ContentPackState? TryGetByRootPath(string rootPath)
    {
        return _packs.Values.FirstOrDefault(pack => string.Equals(pack.RootPath, rootPath, StringComparison.OrdinalIgnoreCase));
    }

    public void Enable(string packId) => Get(packId).Enable();
    public void Disable(string packId) => Get(packId).Disable();

    public IReadOnlyList<ContentPackToggleState> GetSnapshot()
        => _packs.Values.Select(pack => new ContentPackToggleState(pack.Manifest.Id, pack.Enabled)).ToList();

    public void RestoreSnapshot(IEnumerable<ContentPackToggleState> snapshot)
    {
        foreach (var toggle in snapshot)
        {
            if (_packs.TryGetValue(toggle.PackId, out var pack))
            {
                if (toggle.Enabled)
                {
                    pack.Enable();
                }
                else
                {
                    pack.Disable();
                }
            }
        }
    }
}
