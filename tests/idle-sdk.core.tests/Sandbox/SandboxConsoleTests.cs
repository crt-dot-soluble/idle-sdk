using IdleSdk.Core.Sandbox;

namespace IdleSdk.Core.Tests.Sandbox;

public class SandboxConsoleTests
{
    [Fact]
    public void Sandbox_Disabled_Rejects_Command()
    {
        var console = new SandboxConsole();

        var result = console.Execute(new SandboxCommand("noop", new Dictionary<string, string>()));

        Assert.False(result.Success);
    }

    [Fact]
    public void Sandbox_Executes_Registered_Command()
    {
        var console = new SandboxConsole();
        console.Enable();
        console.Register("ping", _ => new SandboxResult(true, "pong"));

        var result = console.Execute(new SandboxCommand("ping", new Dictionary<string, string>()));

        Assert.True(result.Success);
        Assert.Equal("pong", result.Message);
    }
}
