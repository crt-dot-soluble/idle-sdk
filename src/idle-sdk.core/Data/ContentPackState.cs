namespace IdleSdk.Core.Data;

public sealed class ContentPackState
{
    public ContentPackState(ContentPackManifest manifest, string rootPath, IReadOnlyDictionary<string, string> dataFiles)
    {
        Manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        DataFiles = dataFiles ?? throw new ArgumentNullException(nameof(dataFiles));
        Enabled = manifest.EnabledByDefault;
    }

    public ContentPackManifest Manifest { get; }
    public string RootPath { get; }
    public IReadOnlyDictionary<string, string> DataFiles { get; }
    public bool Enabled { get; private set; }

    public void Enable() => Enabled = true;
    public void Disable() => Enabled = false;
}
