using IdleSdk.Core.Actions;

namespace IdleSdk.Core.Tests.Actions;

public class ActionRegistryTests
{
    [Fact]
    public void Registry_Registers_Definitions_And_Handlers()
    {
        var registry = new ActionRegistry();
        var definition = new ActionDefinition("train", "Train", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1), Array.Empty<string>());
        var handler = new TestHandler("train");

        registry.RegisterDefinition(definition);
        registry.RegisterHandler(handler);

        Assert.Equal(definition, registry.GetDefinition("train"));
        Assert.Equal(handler, registry.GetHandler("train"));
    }

    [Fact]
    public void Runner_Invokes_Handler()
    {
        var registry = new ActionRegistry();
        registry.RegisterDefinition(new ActionDefinition("idle", "Idle", TimeSpan.FromSeconds(1), TimeSpan.Zero, Array.Empty<string>()));
        registry.RegisterHandler(new TestHandler("idle"));

        var runner = new ActionRunner(registry);
        var result = runner.Step("idle", new ActionContext(Guid.NewGuid(), DateTimeOffset.UtcNow), TimeSpan.FromSeconds(1));

        Assert.True(result.Completed);
    }

    [Fact]
    public void Runner_Respects_Cooldown()
    {
        var registry = new ActionRegistry();
        registry.RegisterDefinition(new ActionDefinition("test", "Test", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), Array.Empty<string>()));
        registry.RegisterHandler(new TestHandler("test"));

        var runner = new ActionRunner(registry);
        var start = DateTimeOffset.UtcNow;

        var first = runner.Step("test", new ActionContext(Guid.NewGuid(), start), TimeSpan.FromSeconds(1));
        var second = runner.Step("test", new ActionContext(Guid.NewGuid(), start.AddSeconds(1)), TimeSpan.FromSeconds(1));

        Assert.True(first.Completed);
        Assert.False(second.Completed);
        Assert.Equal("cooldown", second.Output);
    }

    [Fact]
    public void Runner_Applies_Modifiers()
    {
        var registry = new ActionRegistry();
        registry.RegisterDefinition(new ActionDefinition("test", "Test", TimeSpan.FromSeconds(1), TimeSpan.Zero, new[] { "tag" }));
        registry.RegisterHandler(new TestHandler("test"));

        var runner = new ActionRunner(registry);
        runner.RegisterModifier(new OutputModifier());

        var result = runner.Step("test", new ActionContext(Guid.NewGuid(), DateTimeOffset.UtcNow), TimeSpan.FromSeconds(1));

        Assert.Equal("modified", result.Output);
    }

    [Fact]
    public void Runner_Roundtrips_Cooldown_State()
    {
        var registry = new ActionRegistry();
        registry.RegisterDefinition(new ActionDefinition("test", "Test", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), Array.Empty<string>()));
        registry.RegisterHandler(new TestHandler("test"));

        var runner = new ActionRunner(registry);
        var start = DateTimeOffset.UtcNow;
        runner.Step("test", new ActionContext(Guid.NewGuid(), start), TimeSpan.FromSeconds(1));

        var snapshot = runner.GetCooldownSnapshot();

        var restored = new ActionRunner(registry);
        restored.RestoreCooldownSnapshot(snapshot);

        var result = restored.Step("test", new ActionContext(Guid.NewGuid(), start.AddSeconds(1)), TimeSpan.FromSeconds(1));

        Assert.False(result.Completed);
    }

    private sealed class TestHandler : IActionHandler
    {
        public TestHandler(string actionId)
        {
            ActionId = actionId;
        }

        public string ActionId { get; }

        public ActionResult Execute(ActionContext context, TimeSpan delta)
        {
            return new ActionResult(true, "ok");
        }
    }

    private sealed class OutputModifier : IActionModifier
    {
        public ActionResult Apply(ActionContext context, ActionResult result, ActionDefinition definition)
        {
            return result with { Output = "modified" };
        }
    }
}
