using IdleSdk.Core.Data;
using IdleSdk.Core.Events;

namespace IdleSdk.Core.Tests.Data;

public class ContentPackManagerTests
{
    [Fact]
    public async Task LoadFromDirectory_Validates_Manifest_And_Data()
    {
        var root = CreateTempDirectory();
        var packDir = Path.Combine(root, "pack-a");
        Directory.CreateDirectory(Path.Combine(packDir, "data"));

        var manifest = """
        {
          "id": "pack-a",
          "name": "Pack A",
          "version": "1.0.0",
          "schemaVersion": "1.0",
          "dependencies": [],
          "modules": [],
          "enabledByDefault": true
        }
        """;

        var actions = """
        [
          { "id": "gather", "name": "Gather", "duration": 1, "cooldown": 0, "tags": ["basic"] }
        ]
        """;

        File.WriteAllText(Path.Combine(packDir, "pack.json"), manifest);
        File.WriteAllText(Path.Combine(packDir, "data", "actions.json"), actions);

        var registry = new ContentPackRegistry();
        var manifestValidator = await DataPackValidator.ForContentPackManifestAsync();
        var actionValidator = await DataPackValidator.FromSchemaJsonAsync(DataPackSchemas.ActionDataSchema);
        var manager = new ContentPackManager(registry, manifestValidator, new Dictionary<string, DataPackValidator>
        {
            ["actions"] = actionValidator
        }, new EventHub());

        var result = manager.LoadFromDirectory(packDir);

        Assert.True(result.IsValid);
        Assert.NotNull(result.Pack);
        Assert.Equal("pack-a", result.Pack!.Manifest.Id);
    }

    [Fact]
    public async Task LoadFromDirectory_Fails_On_Invalid_Manifest()
    {
        var root = CreateTempDirectory();
        var packDir = Path.Combine(root, "pack-b");
        Directory.CreateDirectory(packDir);

        File.WriteAllText(Path.Combine(packDir, "pack.json"), "{ \"name\": \"MissingId\" }");

        var registry = new ContentPackRegistry();
        var manifestValidator = await DataPackValidator.ForContentPackManifestAsync();
        var manager = new ContentPackManager(registry, manifestValidator);

        var result = manager.LoadFromDirectory(packDir);

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task Registry_Snapshot_Roundtrip()
    {
        var registry = new ContentPackRegistry();
        var manifestValidator = await DataPackValidator.ForContentPackManifestAsync();
        var manager = new ContentPackManager(registry, manifestValidator);

        var root = CreateTempDirectory();
        var packDir = Path.Combine(root, "pack-c");
        Directory.CreateDirectory(packDir);
        File.WriteAllText(Path.Combine(packDir, "pack.json"), """
        {
          "id": "pack-c",
          "name": "Pack C",
          "version": "1.0.0",
          "schemaVersion": "1.0",
          "enabledByDefault": false
        }
        """);

        var result = manager.LoadFromDirectory(packDir);
        registry.Register(result.Pack!);
        manager.Enable("pack-c");

        var snapshot = registry.GetSnapshot();
        registry.Disable("pack-c");
        registry.RestoreSnapshot(snapshot);

        Assert.True(registry.Get("pack-c").Enabled);
    }

    private static string CreateTempDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), $"idle-sdk-{Guid.NewGuid()}");
        Directory.CreateDirectory(root);
        return root;
    }
}
