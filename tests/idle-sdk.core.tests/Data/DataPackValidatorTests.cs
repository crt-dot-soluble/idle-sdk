using IdleSdk.Core.Data;

namespace IdleSdk.Core.Tests.Data;

public class DataPackValidatorTests
{
    [Fact]
    public async Task ValidateJson_ReturnsErrors_WhenInvalid()
    {
        var schemaJson = """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "properties": {
            "name": { "type": "string" },
            "version": { "type": "string" }
          },
          "required": ["name", "version"],
          "additionalProperties": false
        }
        """;

        var validator = await DataPackValidator.FromSchemaJsonAsync(schemaJson);
        var result = validator.ValidateJson("{\"name\":123}");

        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateJson_Succeeds_WhenValid()
    {
        var schemaJson = """
        {
          "$schema": "https://json-schema.org/draft/2020-12/schema",
          "type": "object",
          "properties": {
            "name": { "type": "string" },
            "version": { "type": "string" }
          },
          "required": ["name", "version"],
          "additionalProperties": false
        }
        """;

        var validator = await DataPackValidator.FromSchemaJsonAsync(schemaJson);
        var result = validator.ValidateJson("{\"name\":\"pack\",\"version\":\"1\"}");

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
