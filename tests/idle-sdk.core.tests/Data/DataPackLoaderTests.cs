using IdleSdk.Core.Data;

namespace IdleSdk.Core.Tests.Data;

public class DataPackLoaderTests
{
    [Fact]
    public async Task Loader_Returns_Errors_For_Invalid_Pack()
    {
        var schemaJson = """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "properties": {
            "name": { "type": "string" }
          },
          "required": ["name"]
        }
        """;

        var validator = await DataPackValidator.FromSchemaJsonAsync(schemaJson);
        var loader = new DataPackLoader(validator);

        var result = loader.LoadFromJson("{}");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }
}
