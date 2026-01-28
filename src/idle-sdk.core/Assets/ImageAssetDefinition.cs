namespace IdleSdk.Core.Assets;

/// <summary>
/// Defines an image asset that can be referenced by ID and resolved by a renderer.
/// The engine does not load pixels directly; it tracks IDs and source paths.
/// </summary>
public sealed record ImageAssetDefinition(
    string Id,
    string Path,
    ImageAssetType Type,
    int? RenderWidth = null,
    int? RenderHeight = null
);

/// <summary>
/// Supported image asset types for icon and sprite sources.
/// </summary>
public enum ImageAssetType
{
    /// <summary>Unknown or custom image type.</summary>
    Unknown,
    /// <summary>Portable Network Graphics.</summary>
    Png,
    /// <summary>JPEG image.</summary>
    Jpg,
    /// <summary>JPEG image with .jpeg extension.</summary>
    Jpeg
}
