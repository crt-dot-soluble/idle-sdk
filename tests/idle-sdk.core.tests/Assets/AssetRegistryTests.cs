using IdleSdk.Core.Assets;

namespace IdleSdk.Core.Tests.Assets;

public class AssetRegistryTests
{
    [Fact]
    public void Register_And_Get_Image_Asset()
    {
        var registry = new AssetRegistry();
        var image = new ImageAssetDefinition("icon-idle", "icons/idling.png", ImageAssetType.Png, 32, 32);

        registry.RegisterImage(image);
        var fetched = registry.GetImage("icon-idle");

        Assert.Equal("icons/idling.png", fetched.Path);
        Assert.Equal(ImageAssetType.Png, fetched.Type);
        Assert.Equal(32, fetched.RenderWidth);
        Assert.Equal(32, fetched.RenderHeight);
    }

    [Fact]
    public void SpriteSheet_Grid_Creates_Frames()
    {
        var sheet = new SpriteSheetDefinition("sheet", "atlas", 32, 32, 2, 2);

        Assert.Equal(4, sheet.Frames.Count);
        Assert.Equal(0, sheet.Frames[0].X);
        Assert.Equal(32, sheet.Frames[1].X);
        Assert.Equal(32, sheet.Frames[2].Y);
    }

    [Fact]
    public void Registry_Resolves_Frame_By_Id()
    {
        var registry = new AssetRegistry();
        registry.RegisterImage(new ImageAssetDefinition("atlas", "sprites/atlas.png", ImageAssetType.Png));
        registry.RegisterSpriteSheet(new SpriteSheetDefinition("sheet", "atlas", 16, 16, 1, 1));

        var frame = registry.GetFrame("sheet", "frame-0");

        Assert.Equal(0, frame.X);
        Assert.Equal(0, frame.Y);
        Assert.Equal(16, frame.Width);
        Assert.Equal(16, frame.Height);
    }
}
