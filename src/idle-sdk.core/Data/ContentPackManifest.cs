using System.Text.Json.Serialization;

namespace IdleSdk.Core.Data;

public sealed record ContentPackManifest(
    string Id,
    string Name,
    string Version,
    string SchemaVersion,
    IReadOnlyList<string> Dependencies,
    IReadOnlyList<string> Modules,
    bool EnabledByDefault
)
{
    public static ContentPackManifest CreateDefault(string id, string name)
        => new(id, name, "1.0.0", "1.0", Array.Empty<string>(), Array.Empty<string>(), true);
}
