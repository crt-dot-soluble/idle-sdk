# Engine API Overview

This document summarizes the public API surface to help developers extend the engine, create content packs, and integrate custom systems. It is derived from the engine’s public types and inline constraints.

## Core Loop & Timing
- SimulationClock: deterministic time source
- TickScheduler: fixed-step scheduler for simulation ticks
- OfflineReconciler: reconciles offline time into deterministic ticks

## Events
- EventHub: strongly-typed publish/subscribe event bus
- EventSubscription: disposable subscription handle

## Modules & Plugins
- IModule: module contract for custom systems
- ModuleRegistry: register modules and resolve dependency order
- ModuleLoader: load and initialize modules

## Actions
- ActionDefinition: action metadata (duration, cooldown, tags)
- ActionRegistry: register action definitions and handlers
- IActionHandler: action execution contract
- ActionRunner: executes actions and applies modifiers
- IActionModifier: post-processing hook for action results
- ActionCooldownState: snapshot/restore helper

## Skills
- SkillDefinition: skill metadata + unlock milestones
- SkillRegistry: register skills
- SkillSystem: skill progression manager
- SkillProgress: XP/level tracking and unlocks
- IXpCurve + LinearXpCurve: XP curve implementations

## Combat
- CombatSystem: tick-based combat stepper
- CombatEncounter: encounter state, AI targeting, damage rules
- CombatantState/CombatantStats: combat actor data
- StatusEffect + CombatantEffects: status effects + stacking
- CombatRules: caps/bonuses/stacking
- ICombatAi + SimpleCombatAi: AI target selection

## Data & Content Packs
- DataPackValidator: JSON schema validation
- DataPackSchemas: bundled schemas for common content files
- DataPackLoader: JSON validation wrapper
- ContentPackManifest: pack.json manifest model
- ContentPackManager: load/validate/hot-reload packs
- ContentPackRegistry: enable/disable + snapshots

## Assets
- ImageAssetDefinition: image metadata (id/path/type)
- SpriteSheetDefinition: sheet layout + frames
- SpriteFrameDefinition: frame rectangle
- AssetRegistry: image + sprite sheet registry

## Persistence
- ISnapshotStore: storage interface
- SqliteSnapshotStore: SQLite implementation
- IStateSerializer + JsonStateSerializer: serialization
- SnapshotService: save/load facade

## Sandbox
- SandboxConsole: command registry and execution
- RuntimeInspector: state inspection utilities

## Demo App
- DemoViewModel: sample integration of systems
- MainWindow/App: Avalonia UI host

## Extension Guidance
- Prefer content packs for data-only additions.
- Use modules for new logic, integration, or tooling.
- Use EventHub for cross-system hooks.
