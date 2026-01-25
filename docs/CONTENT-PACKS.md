# Content Packs (No-Code)

Content packs are JSON-only bundles that define gameplay content without code changes. Packs can be enabled/disabled and hot-reloaded at runtime.

## Directory Structure
- pack.json (manifest)
- data/*.json (typed content files)

Example:

```
content-packs/
  starter-pack/
    pack.json
    data/
      actions.json
      skills.json
      items.json
      quests.json
      worlds.json
      combat-ai.json
      status-effects.json
```

## Manifest (pack.json)
Required fields:
- id
- name
- version
- schemaVersion

Optional:
- dependencies (array of pack ids)
- modules (array of module names)
- enabledByDefault (boolean)

Example:
```
{
  "id": "starter-pack",
  "name": "Starter Pack",
  "version": "1.0.0",
  "schemaVersion": "1.0",
  "dependencies": [],
  "modules": [],
  "enabledByDefault": true
}
```

## Data Files
The engine validates data files using schemas declared in the data layer:
- actions.json
- skills.json
- status-effects.json
- combat-ai.json

Each file is validated independently. Invalid files prevent the pack from loading.

## Runtime Enable/Disable
Content packs are registered in the ContentPackRegistry. Packs can be toggled at runtime:
- Enable(packId)
- Disable(packId)

The registry exposes snapshot helpers so pack toggle state can be persisted per profile.

## Hot Reload
ContentPackManager can watch the pack root directory and hot-reload packs when JSON files change. Reload events emit audit signals through the event bus so the app can rebuild registries or refresh UI.

## Sandbox & Security
- Content packs cannot execute code.
- Pack paths are restricted to engine-owned directories.
- All packs are validated against schemas prior to activation.

## Recommended Workflow
1. Author pack.json and data/*.json
2. Validate with ContentPackManager
3. Enable pack in the registry
4. Use hot reload during development
