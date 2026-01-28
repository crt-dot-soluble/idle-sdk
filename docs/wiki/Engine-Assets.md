# Engine Assets

The engine now supports image assets and sprite sheets at the core API level.

## Capabilities
- Register PNG/JPG image sources with IDs.
- Define sprite sheets by grid layout or explicit frames.
- Resolve frames by id or index for renderers.
- Optional renderWidth/renderHeight for upscaling in UI.

## Content Packs
Data packs can include:
- data/images.json
- data/sprite-sheets.json

These files map image IDs to paths and declare sprite sheet layouts.
