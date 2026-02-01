
# IMPLEMENTATION SPECIFICATION — idle-sdk Engine Core

> This specification is authoritative for implementation. It supersedes narrative descriptions and must be kept in sync with the engine design.

## Overview
- Purpose: Deliver a modular, engine-first idle RPG framework (`idle-sdk`) for building game-agnostic idle and semi-idle RPGs, targeting .NET 10, with deterministic simulation and plugin extensibility.
- Scope: Engine core and system architecture across simulation, module/plugin loading, data-driven gameplay systems, persistence, and developer sandbox. The first game (`idle-idol`) is out of scope and will be implemented in a separate project using this engine.
- Non-goals:
   - Shipping a complete game experience.
   - Hard-coding gameplay rules from any specific idle RPG.
   - Implementing networked multiplayer features in the core.

## Requirements
- Functional requirements:
   - FR-1: Provide a deterministic simulation loop with fixed timestep execution and offline time reconciliation.
   - FR-2: Provide a strongly-typed event bus for cross-module communication with publish/subscribe semantics.
   - FR-3: Provide a module/plugin system with discovery, versioned dependencies, and safe hot-load capabilities.
   - FR-4: Define data-driven action, skill, and combat systems with schema-based configuration.
     - Actions must support cooldowns, tags, and modifier hooks.
     - Skills must support level-based unlock milestones.
     - Combat must support status effects and AI-driven target selection.
   - FR-5: Represent world data with hierarchical structures (Universe → World → Region → Zone → Node) with exploration tracking.
   - FR-6: Provide a scene/renderer interface that consumes simulation state and emits scene layer updates.
   - FR-7: Provide a deterministic layered generator for items/characters/cosmetics based on seeds, rarity weights, and exclusion rules.
   - FR-8: Provide account/profile, inventory, and multi-currency wallet systems with offline-safe accounting.
   - FR-9: Provide a persistence layer with snapshot-based save/load and plugin-safe serialization.
   - FR-10: Provide a developer sandbox for runtime inspection, data reload, and state mutation.
    - FR-11: Provide a content pack system that can be added, enabled/disabled, and hot-reloaded without code changes.
       - Content packs are JSON-based and validated against schemas.
       - Packs are sandboxed (no code execution) and can be dynamically toggled at runtime.
       - Pack changes are surfaced as deterministic reload events.
      - FR-12: Provide a core asset registry for image assets and sprite sheets (PNG/JPG) with frame indexing for renderers.
- Non-functional requirements:
   - NFR-1 (performance): Simulation must sustain at least 60 ticks/second for baseline content on a mid-tier desktop CPU.
   - NFR-2 (security): No arbitrary code execution from data packs; plugin loading must be explicit and versioned.
   - NFR-3 (reliability): Deterministic playback must produce identical results from the same seed and input stream.
- Constraints:
   - Policy constraints: Full test coverage is required; inline documentation is required; CI/CD must be enforced.
   - Platform constraints: .NET 10 target, desktop-first with web compatibility.

## Interfaces
- Inputs (schemas, validation rules):
   - Module manifests: name, version, dependencies, entry points, schema versions.
   - Content packs: pack manifest, module list, data catalog, schema versions, dependencies, and enable/disable flags.
   - Data packs: actions, skills, items, worlds, enemies, loot tables, layers, UI panel descriptors, action modifiers, status effects, combat AI profiles, quests, achievements, collections, audio, trade, crafting, compendium entries, images, and sprite sheets.
   - Runtime configuration: tick rate, offline reconciliation limits, logging level, sandbox permissions.
- Outputs (schemas, guarantees):
   - Simulation state snapshots (versioned, deterministic, diffable).
   - Event stream (typed events with metadata, time, and source).
   - Scene layer updates (background, midground, foreground, overlay) with stable identifiers.
- Error cases (error types, codes, handling):
   - Invalid schema/data pack: validation errors with field-level diagnostics.
   - Plugin dependency conflict: explicit dependency error with conflict resolution guidance.
   - Simulation divergence: deterministic mismatch error with seed/input replay diagnostics.

## Architecture
- Components/modules:
   - Core loop (`SimulationClock`, `TickScheduler`)
   - Event bus (`EventHub`, `EventSubscription`)
   - Module system (`ModuleRegistry`, `ModuleLoader`)
   - Data layer (`SchemaValidator`, `DataPackLoader`, `ContentPackRegistry`, `ContentPackManager`)
   - Gameplay systems (Actions, Skills, Combat, World, Economy, Items)
   - Assets (`AssetRegistry`, `ImageAssetDefinition`, `SpriteSheetDefinition`)
   - Scene interface (`SceneAdapter`, `SceneLayerModel`)
   - Persistence (`SnapshotStore`, `StateSerializer`)
   - Sandbox (`SandboxConsole`, `RuntimeInspector`)
- Data flow:
   - Content packs → validation → data pack load → module registry → system initialization → simulation ticks → event stream → scene updates → persistence snapshots.
- State management:
   - Single authoritative simulation state; mutations occur only during tick execution.
- Dependencies (internal/external):
   - Internal: shared abstractions for time, events, modules, and persistence.
   - External: none mandated; storage provider and renderer are pluggable.

### WebAssembly Integration (Preferred Web Target)
- Runtime: C# compiled to WebAssembly via .NET WASM.
- Execution: Runs directly in the browser, no server round-trips; offline works by design.
- Interop: JavaScript/TypeScript calls into the engine like a library.
- Rendering stack: TS/JS using Pixi for rendering and React for UI.
- Contracts: Shared data contracts serialized as JSON.
- Runtime loop:
   - `tick(delta)` → `engine.step(delta)` → state diff → render
- Debug tooling:
   - Provide optional Three.js-based 3D model previews for tooling/debug panels.

## Data & Storage
- Content pack format:
   - `pack.json` (manifest): id, name, version, schemaVersion, dependencies, modules, enabledByDefault.
   - `data/*.json`: typed content documents (actions, skills, quests, items, worlds, etc.).
   - Packs must be loadable without code and hot-reloadable at runtime.
   - Enable/disable toggles must be reversible without application restart.
- Data models:
   - Accounts, Profiles, Wallets, Inventories, Worlds, Nodes, Items, Actions, Skills, Skill Unlocks, Combat Encounters, Status Effects, Combat AI Profiles, Achievements, Content Pack Manifests, Image Assets, Sprite Sheets, Sprite Frames.
- Persistence requirements:
   - Snapshot-based saves with versioned schema.
   - Deterministic replay input log for validation and debugging.
   - Content pack enable/disable state persisted per profile.
- Retention/cleanup:
   - Configurable snapshot retention (default: last 5 snapshots per profile).

## Security & Compliance
- Threat model summary: prevent untrusted data packs from executing code; enforce deterministic seed replay; protect profile data at rest.
- Access controls: sandbox actions require explicit enabling; file I/O restricted to engine-owned paths.
- Content pack sandboxing: content packs cannot execute code, only declare data; pack paths are restricted to engine-owned directories.
- Audit/telemetry: event bus supports audit events; persistence logs include schema versions and checksums.

## Observability
- Logging requirements: structured logs for tick lifecycle, module load/unload, and persistence operations.
- Metrics: tick duration, event throughput, save/load duration, offline reconciliation time.
- Tracing: optional, pluggable tracing interface for long-running actions.
- Content pack lifecycle events: pack load, enable, disable, and hot-reload must emit audit events.

## Acceptance Criteria
- Tests to pass:
   - Deterministic replay tests for fixed seeds and event streams.
   - Schema validation tests for data packs.
   - Module load/unload lifecycle tests.
   - Content pack validation, enable/disable, and hot-reload tests.
- Success metrics:
   - Simulation ticks at or above target rate under baseline content.
   - Zero deterministic divergence across replay runs.
- Performance thresholds:
   - Offline reconciliation for 24 hours of simulation completes within 2 seconds on baseline hardware.
- Security checks:
   - Data packs cannot execute code paths outside approved module interfaces.

## Test Plan
- Unit tests:
   - Tick scheduling, event routing, schema validation, serializer integrity, action cooldown gating, action modifier application, status effect ticking/expiry, AI target selection.
- Integration tests:
   - Module registration with dependencies; offline progression reconciliation; content pack enable/disable and hot reload.
- Acceptance tests:
   - End-to-end simulation replay with known outputs.

## Demo Implementation Requirements
- Demo goal: Minimal Melvor-style idle clone showcasing engine capabilities with a game view area reserved for future background/character/equipment sprites.
- UI stack: Avalonia desktop application.
- Game view: Fixed panel with placeholder background and character silhouette; exposes scene layer entries and diffs.
- Required feature wiring (minimum):
   - Actions + Skills + Combat (tick-based loop, XP gains, basic combat tick output).
   - World + Scene layer model (universe/world/region/zone/node presentation, scene diff output).
   - Inventory + Equipment + Economy (currency wallet, items, gear slots).
   - Quests + Achievements + Collections (progress updates visible in UI).
- Data: In-memory demo data packs (no external content required).
- Web demo tooling:
   - Provide a debug-panel 3D viewport using Three.js to preview GLB models for validation.
   - Provide a debug-only expanded 3D viewer pane that opens from model previews/icons when explicitly enabled in the debug module controls.
     - Interactions: orbit rotate, pan, zoom, and reset to default framing.
     - The expanded viewer must be modular and swappable to support a future “hover-to-expand” surface.
   - Display modes must default to responsive auto selection, while manual overrides remain available for debugging.

## Rollout & Migration
- Migration steps:
   - Versioned schema upgrades with migration hooks per module.
- Backout plan:
   - Restore previous snapshot and schema version; disable offending module.

## Open Questions
- None. Decisions recorded in docs/ARCHITECTURE-DECISIONS.md:
   - Renderer: WebAssembly (C# → WASM + TS/JS renderer).
   - Persistence: SQLite.
   - Plugin packaging: NuGet package.
