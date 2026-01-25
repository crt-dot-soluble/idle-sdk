namespace IdleSdk.Core.Data;

public static class DataPackSchemas
{
    public const string ContentPackManifestSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "object",
      "required": ["id", "name", "version", "schemaVersion"],
      "properties": {
        "id": { "type": "string", "minLength": 1 },
        "name": { "type": "string", "minLength": 1 },
        "version": { "type": "string", "minLength": 1 },
        "schemaVersion": { "type": "string", "minLength": 1 },
        "dependencies": { "type": "array", "items": { "type": "string" } },
        "modules": { "type": "array", "items": { "type": "string" } },
        "enabledByDefault": { "type": "boolean" }
      }
    }
    """;

    public const string ActionDataSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "name", "duration"],
        "properties": {
          "id": { "type": "string" },
          "name": { "type": "string" },
          "duration": { "type": "number", "minimum": 0 },
          "cooldown": { "type": "number", "minimum": 0 },
          "tags": { "type": "array", "items": { "type": "string" } }
        }
      }
    }
    """;

    public const string SkillDataSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "name", "maxLevel"],
        "properties": {
          "id": { "type": "string" },
          "name": { "type": "string" },
          "maxLevel": { "type": "integer", "minimum": 1 },
          "unlocks": {
            "type": "object",
            "additionalProperties": { "type": "string" }
          }
        }
      }
    }
    """;

    public const string StatusEffectSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "durationTicks"],
        "properties": {
          "id": { "type": "string" },
          "durationTicks": { "type": "integer", "minimum": 1 },
          "attackDelta": { "type": "integer" },
          "defenseDelta": { "type": "integer" }
        }
      }
    }
    """;

    public const string CombatAiSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "strategy"],
        "properties": {
          "id": { "type": "string" },
          "strategy": { "type": "string" },
          "parameters": { "type": "object" }
        }
      }
    }
    """;
}
