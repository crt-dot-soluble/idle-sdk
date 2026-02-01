using IdleSdk.Core.Sandbox;
using System;

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

    [Fact]
    public void Inspector_Lists_Registered_Properties()
    {
        var inspector = new RuntimeInspector();
        inspector.RegisterProperty("tickRate", typeof(double), () => 1.0, null, "Tick speed");

        var properties = inspector.ListProperties();

        Assert.Contains(properties, prop => prop.Name == "tickRate" && prop.IsWritable == false);
    }

    [Fact]
    public void Inspector_Can_Write_When_Writable()
    {
        var inspector = new RuntimeInspector();
        var value = 1.0;
        inspector.RegisterProperty("tickRate", typeof(double), () => value, next => value = Convert.ToDouble(next), "Tick speed");

        inspector.Write("tickRate", 2.5);

        Assert.Equal(2.5, value);
    }
}
