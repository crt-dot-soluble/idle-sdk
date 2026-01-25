using IdleSdk.Core.Persistence;

namespace IdleSdk.Core.Tests.Persistence;

public class SnapshotServiceTests
{
    private sealed record DemoState(int Gold, int Logs);

    [Fact]
    public async Task SnapshotService_Saves_And_Loads_State()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"idle-sdk-snapshot-{Guid.NewGuid():N}.db");
        var store = new SqliteSnapshotStore($"Data Source={tempPath}");
        var serializer = new JsonStateSerializer<DemoState>();
        var service = new SnapshotService<DemoState>(store, serializer);

        var state = new DemoState(5, 2);
        await service.SaveAsync("profile", state, "v1");

        var loaded = await service.LoadLatestAsync("profile");

        Assert.NotNull(loaded);
        Assert.Equal(5, loaded!.Gold);

        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(tempPath))
        {
            File.Delete(tempPath);
        }
    }
}
