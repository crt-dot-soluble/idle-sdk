namespace IdleSdk.Core.Assets;

/// <summary>
/// Defines a single sprite frame within a sprite sheet.
/// </summary>
public sealed record SpriteFrameDefinition(
    string Id,
    int X,
    int Y,
    int Width,
    int Height
);
