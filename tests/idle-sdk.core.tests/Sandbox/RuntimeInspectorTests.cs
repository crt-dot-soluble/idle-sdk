using IdleSdk.Core.Sandbox;

namespace IdleSdk.Core.Tests.Sandbox;

public class RuntimeInspectorTests
{
    [Fact]
    public void Inspector_Returns_Registered_Value()
    {
        var inspector = new RuntimeInspector();
        inspector.RegisterReader("version", () => "1.0");

        var value = inspector.Read("version");

        Assert.Equal("1.0", value);
    }
}
