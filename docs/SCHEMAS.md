# JSON Schemas

This document lists the supported content pack JSON schemas in the engine.

## Manifest
- Source: DataPackSchemas.ContentPackManifestSchema
- File: pack.json
- Required: id, name, version, schemaVersion
- Optional: dependencies, modules, enabledByDefault

## Actions
- Source: DataPackSchemas.ActionDataSchema
- File: data/actions.json
- Fields: id, name, duration, cooldown, tags

## Skills
- Source: DataPackSchemas.SkillDataSchema
- File: data/skills.json
- Fields: id, name, icon, maxLevel, unlocks (level->string)

## Status Effects
- Source: DataPackSchemas.StatusEffectSchema
- File: data/status-effects.json
- Fields: id, durationTicks, attackDelta, defenseDelta

## Combat AI
- Source: DataPackSchemas.CombatAiSchema
- File: data/combat-ai.json
- Fields: id, strategy, parameters

## Images
- Source: DataPackSchemas.ImageAssetSchema
- File: data/images.json
- Fields: id, path, type (png/jpg/jpeg/unknown), renderWidth?, renderHeight?

## Sprite Sheets
- Source: DataPackSchemas.SpriteSheetSchema
- File: data/sprite-sheets.json
- Fields: id, imageId, frameWidth, frameHeight, columns, rows, frames[]

## Validation Rules
- Each JSON file is validated independently.
- Invalid content blocks pack load.
- Validation errors return field-level diagnostics.
