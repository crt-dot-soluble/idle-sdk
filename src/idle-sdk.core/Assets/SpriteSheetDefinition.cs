namespace IdleSdk.Core.Assets;

/// <summary>
/// Defines a sprite sheet backed by an image asset and either explicit frames or a grid layout.
/// </summary>
public sealed class SpriteSheetDefinition
{
    public SpriteSheetDefinition(
        string id,
        string imageId,
        int frameWidth,
        int frameHeight,
        int columns,
        int rows,
        IReadOnlyList<SpriteFrameDefinition>? frames = null)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Sheet id must be provided.", nameof(id));
        }
        if (string.IsNullOrWhiteSpace(imageId))
        {
            throw new ArgumentException("Image id must be provided.", nameof(imageId));
        }
        if (frameWidth <= 0 || frameHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(frameWidth), "Frame size must be positive.");
        }
        if (columns <= 0 || rows <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(columns), "Grid dimensions must be positive.");
        }

        Id = id;
        ImageId = imageId;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Columns = columns;
        Rows = rows;
        Frames = frames is not null && frames.Count > 0 ? frames : BuildGridFrames(frameWidth, frameHeight, columns, rows);
    }

    /// <summary>Unique id of the sprite sheet.</summary>
    public string Id { get; }

    /// <summary>Image asset id that backs the sheet.</summary>
    public string ImageId { get; }

    /// <summary>Width of a single frame in pixels.</summary>
    public int FrameWidth { get; }

    /// <summary>Height of a single frame in pixels.</summary>
    public int FrameHeight { get; }

    /// <summary>Number of columns in the grid.</summary>
    public int Columns { get; }

    /// <summary>Number of rows in the grid.</summary>
    public int Rows { get; }

    /// <summary>Resolved frames for the sheet.</summary>
    public IReadOnlyList<SpriteFrameDefinition> Frames { get; }

    /// <summary>Get a frame by id.</summary>
    public SpriteFrameDefinition GetFrame(string frameId)
    {
        var frame = Frames.FirstOrDefault(candidate => candidate.Id.Equals(frameId, StringComparison.OrdinalIgnoreCase));
        if (frame is null)
        {
            throw new KeyNotFoundException($"Frame '{frameId}' was not found in sheet '{Id}'.");
        }

        return frame;
    }

    /// <summary>Get a frame by grid index (row-major).</summary>
    public SpriteFrameDefinition GetFrame(int index)
    {
        if (index < 0 || index >= Frames.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        return Frames[index];
    }

    private static IReadOnlyList<SpriteFrameDefinition> BuildGridFrames(int frameWidth, int frameHeight, int columns, int rows)
    {
        var frames = new List<SpriteFrameDefinition>(columns * rows);
        var index = 0;
        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                frames.Add(new SpriteFrameDefinition($"frame-{index}", col * frameWidth, row * frameHeight, frameWidth, frameHeight));
                index++;
            }
        }

        return frames;
    }
}
