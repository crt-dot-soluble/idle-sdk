namespace IdleSdk.Core.Assets;

/// <summary>
/// Registry for image and sprite sheet assets.
/// </summary>
public sealed class AssetRegistry
{
    private readonly Dictionary<string, ImageAssetDefinition> _images = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SpriteSheetDefinition> _sheets = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Register an image asset.</summary>
    public void RegisterImage(ImageAssetDefinition image)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (_images.ContainsKey(image.Id))
        {
            throw new InvalidOperationException($"Image asset '{image.Id}' is already registered.");
        }

        _images[image.Id] = image;
    }

    /// <summary>Register a sprite sheet.</summary>
    public void RegisterSpriteSheet(SpriteSheetDefinition sheet)
    {
        if (sheet is null)
        {
            throw new ArgumentNullException(nameof(sheet));
        }

        if (_sheets.ContainsKey(sheet.Id))
        {
            throw new InvalidOperationException($"Sprite sheet '{sheet.Id}' is already registered.");
        }

        _sheets[sheet.Id] = sheet;
    }

    /// <summary>Get a registered image asset.</summary>
    public ImageAssetDefinition GetImage(string imageId)
    {
        if (!_images.TryGetValue(imageId, out var image))
        {
            throw new KeyNotFoundException($"Image asset '{imageId}' was not found.");
        }

        return image;
    }

    /// <summary>Get a registered sprite sheet.</summary>
    public SpriteSheetDefinition GetSpriteSheet(string sheetId)
    {
        if (!_sheets.TryGetValue(sheetId, out var sheet))
        {
            throw new KeyNotFoundException($"Sprite sheet '{sheetId}' was not found.");
        }

        return sheet;
    }

    /// <summary>Get a frame from a sprite sheet by id.</summary>
    public SpriteFrameDefinition GetFrame(string sheetId, string frameId)
        => GetSpriteSheet(sheetId).GetFrame(frameId);

    /// <summary>Get a frame from a sprite sheet by index.</summary>
    public SpriteFrameDefinition GetFrame(string sheetId, int index)
        => GetSpriteSheet(sheetId).GetFrame(index);
}
