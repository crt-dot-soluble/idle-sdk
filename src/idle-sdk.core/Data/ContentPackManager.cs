using System.Text.Json;
using IdleSdk.Core.Events;

namespace IdleSdk.Core.Data;

public sealed class ContentPackManager
{
    private readonly ContentPackRegistry _registry;
    private readonly DataPackValidator _manifestValidator;
    private readonly IReadOnlyDictionary<string, DataPackValidator> _dataValidators;
    private readonly EventHub? _events;
    private readonly Dictionary<string, DateTimeOffset> _reloadGate = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<FileSystemWatcher> _watchers = new();

    public ContentPackManager(
        ContentPackRegistry registry,
        DataPackValidator manifestValidator,
        IReadOnlyDictionary<string, DataPackValidator>? dataValidators = null,
        EventHub? events = null)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _manifestValidator = manifestValidator ?? throw new ArgumentNullException(nameof(manifestValidator));
        _dataValidators = dataValidators ?? new Dictionary<string, DataPackValidator>(StringComparer.OrdinalIgnoreCase);
        _events = events;
    }

    public ContentPackLoadResult LoadFromDirectory(string packDirectory)
    {
        if (string.IsNullOrWhiteSpace(packDirectory))
        {
            throw new ArgumentException("Pack directory must be provided.", nameof(packDirectory));
        }

        if (!Directory.Exists(packDirectory))
        {
            throw new DirectoryNotFoundException(packDirectory);
        }

        var manifestPath = Path.Combine(packDirectory, "pack.json");
        if (!File.Exists(manifestPath))
        {
            return ContentPackLoadResult.Failure(new[] { new ValidationError("pack.json", "Missing pack.json manifest.") });
        }

        var manifestJson = File.ReadAllText(manifestPath);
        var manifestValidation = _manifestValidator.ValidateJson(manifestJson);
        if (!manifestValidation.IsValid)
        {
            return ContentPackLoadResult.Failure(manifestValidation.Errors);
        }

        var manifest = JsonSerializer.Deserialize<ContentPackManifest>(manifestJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (manifest is null)
        {
            return ContentPackLoadResult.Failure(new[] { new ValidationError("pack.json", "Manifest could not be parsed.") });
        }

        manifest = new ContentPackManifest(
            manifest.Id,
            manifest.Name,
            manifest.Version,
            manifest.SchemaVersion,
            manifest.Dependencies ?? Array.Empty<string>(),
            manifest.Modules ?? Array.Empty<string>(),
            manifest.EnabledByDefault);

        var dataDirectory = Path.Combine(packDirectory, "data");
        var dataFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<ValidationError>();

        if (Directory.Exists(dataDirectory))
        {
            foreach (var file in Directory.GetFiles(dataDirectory, "*.json", SearchOption.TopDirectoryOnly))
            {
                var key = Path.GetFileNameWithoutExtension(file);
                var json = File.ReadAllText(file);
                if (_dataValidators.TryGetValue(key, out var validator))
                {
                    var validation = validator.ValidateJson(json);
                    if (!validation.IsValid)
                    {
                        errors.AddRange(validation.Errors.Select(error => new ValidationError($"data/{key}.json", error.Message)));
                        continue;
                    }
                }

                dataFiles[key] = json;
            }
        }

        if (errors.Count > 0)
        {
            return ContentPackLoadResult.Failure(errors);
        }

        var pack = new ContentPackState(manifest, packDirectory, dataFiles);
        return ContentPackLoadResult.Success(pack);
    }

    public IReadOnlyList<ContentPackLoadResult> LoadAllFromRoot(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Root directory must be provided.", nameof(rootDirectory));
        }

        if (!Directory.Exists(rootDirectory))
        {
            throw new DirectoryNotFoundException(rootDirectory);
        }

        var results = new List<ContentPackLoadResult>();
        foreach (var directory in Directory.GetDirectories(rootDirectory))
        {
            var result = LoadFromDirectory(directory);
            results.Add(result);
            if (result.IsValid && result.Pack is not null)
            {
                _registry.Register(result.Pack);
                _events?.Publish(new ContentPackLoaded(result.Pack.Manifest.Id, result.Pack.Enabled));
                if (result.Pack.Enabled)
                {
                    _events?.Publish(new ContentPackEnabled(result.Pack.Manifest.Id));
                }
            }
        }

        return results;
    }

    public void Enable(string packId)
    {
        _registry.Enable(packId);
        _events?.Publish(new ContentPackEnabled(packId));
    }

    public void Disable(string packId)
    {
        _registry.Disable(packId);
        _events?.Publish(new ContentPackDisabled(packId));
    }

    public void StartWatching(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            throw new ArgumentException("Root directory must be provided.", nameof(rootDirectory));
        }

        if (!Directory.Exists(rootDirectory))
        {
            throw new DirectoryNotFoundException(rootDirectory);
        }

        var watcher = new FileSystemWatcher(rootDirectory)
        {
            IncludeSubdirectories = true,
            Filter = "*.json",
            EnableRaisingEvents = true
        };

        watcher.Changed += (_, args) => TryReload(args.FullPath, rootDirectory);
        watcher.Created += (_, args) => TryReload(args.FullPath, rootDirectory);
        watcher.Deleted += (_, args) => TryReload(args.FullPath, rootDirectory);
        watcher.Renamed += (_, args) => TryReload(args.FullPath, rootDirectory);

        _watchers.Add(watcher);
    }

    public void StopWatching()
    {
        foreach (var watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

        _watchers.Clear();
    }

    private void TryReload(string changedPath, string rootDirectory)
    {
        if (!changedPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (changedPath.EndsWith("pack.json", StringComparison.OrdinalIgnoreCase) && !File.Exists(changedPath))
        {
            var removedRoot = Path.GetDirectoryName(changedPath);
            if (!string.IsNullOrWhiteSpace(removedRoot))
            {
                var existing = _registry.TryGetByRootPath(removedRoot);
                if (existing is not null)
                {
                    _registry.Remove(existing.Manifest.Id);
                    _events?.Publish(new ContentPackDisabled(existing.Manifest.Id));
                }
            }

            return;
        }

        var packDirectory = FindPackRoot(changedPath, rootDirectory);
        if (packDirectory is null)
        {
            return;
        }

        if (IsReloadThrottled(packDirectory))
        {
            return;
        }

        var result = LoadFromDirectory(packDirectory);
        if (!result.IsValid || result.Pack is null)
        {
            _events?.Publish(new ContentPackHotReloadFailed(packDirectory));
            return;
        }

        var packId = result.Pack.Manifest.Id;
        if (TryReplace(packId, result.Pack))
        {
            _events?.Publish(new ContentPackHotReloaded(packId));
        }
    }

    private bool TryReplace(string packId, ContentPackState pack)
    {
        try
        {
            if (_registry.Packs.All(existingPack => !string.Equals(existingPack.Manifest.Id, packId, StringComparison.OrdinalIgnoreCase)))
            {
                _registry.Register(pack);
                if (pack.Enabled)
                {
                    _events?.Publish(new ContentPackEnabled(packId));
                }

                return true;
            }

            var existing = _registry.Get(packId);
            var enabled = existing.Enabled;
            _registry.Replace(pack);
            if (enabled)
            {
                _registry.Enable(packId);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool IsReloadThrottled(string packDirectory)
    {
        var now = DateTimeOffset.UtcNow;
        if (_reloadGate.TryGetValue(packDirectory, out var last) && now - last < TimeSpan.FromMilliseconds(250))
        {
            return true;
        }

        _reloadGate[packDirectory] = now;
        return false;
    }

    private static string? FindPackRoot(string changedPath, string rootDirectory)
    {
        var directory = Path.GetDirectoryName(changedPath);
        while (!string.IsNullOrWhiteSpace(directory))
        {
            var manifestPath = Path.Combine(directory, "pack.json");
            if (File.Exists(manifestPath))
            {
                return directory;
            }

            if (string.Equals(directory, rootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }
}
