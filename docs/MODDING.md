# Modding & Extensions

This guide describes how to extend the engine with custom code or data.

## Content Packs (No-Code)
- Use pack.json + data/*.json
- Packs can be enabled/disabled at runtime
- Packs can be hot-reloaded from disk
- Packs are sandboxed and cannot execute code

## Modules (Code)
- Implement IModule for custom systems
- Register with ModuleRegistry
- Use ModuleLoader to resolve dependencies

## Event-Driven Integrations
- Subscribe to EventHub to react to engine changes
- Publish custom events from modules

## Recommended Patterns
- Keep data in content packs
- Keep logic in modules
- Avoid direct UI coupling in core systems

## Safety & Compatibility
- Keep module APIs stable
- Avoid global mutable state
- Use deterministic RNG for any randomized logic
