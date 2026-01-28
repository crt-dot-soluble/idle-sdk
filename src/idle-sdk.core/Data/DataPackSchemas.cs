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
        "required": ["id", "name", "icon", "maxLevel"],
        "properties": {
          "id": { "type": "string" },
          "name": { "type": "string" },
          "icon": { "type": "string", "minLength": 1 },
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

    public const string ImageAssetSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "path", "type"],
        "properties": {
          "id": { "type": "string" },
          "path": { "type": "string" },
          "type": { "type": "string", "enum": ["png", "jpg", "jpeg", "unknown"] },
          "renderWidth": { "type": "integer", "minimum": 1 },
          "renderHeight": { "type": "integer", "minimum": 1 }
        }
      }
    }
    """;

    public const string SpriteSheetSchema = """
    {
      "$schema": "http://json-schema.org/draft-07/schema#",
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "imageId", "frameWidth", "frameHeight", "columns", "rows"],
        "properties": {
          "id": { "type": "string" },
          "imageId": { "type": "string" },
          "frameWidth": { "type": "integer", "minimum": 1 },
          "frameHeight": { "type": "integer", "minimum": 1 },
          "columns": { "type": "integer", "minimum": 1 },
          "rows": { "type": "integer", "minimum": 1 },
          "frames": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["id", "x", "y", "width", "height"],
              "properties": {
                "id": { "type": "string" },
                "x": { "type": "integer", "minimum": 0 },
                "y": { "type": "integer", "minimum": 0 },
                "width": { "type": "integer", "minimum": 1 },
                "height": { "type": "integer", "minimum": 1 }
              }
            }
          }
        }
      }
    }
    """;
}
