using IdleSdk.Core.Replay;

namespace IdleSdk.Core.Tests.Replay;

public class DeterministicReplayRunnerTests
{
    [Fact]
    public void Replay_Is_Deterministic()
    {
        var runner = new DeterministicReplayRunner();
        var first = runner.Run(5, tick => $"{tick}-output");
        var second = runner.Run(5, tick => $"{tick}-output");

        Assert.Equal(first, second);
    }
}
