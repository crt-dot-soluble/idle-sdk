# Modules & Plugins

Modules provide code-driven extensions, while content packs remain data-only. Use modules for advanced logic, custom systems, or integration with external services.

## Module Registration
Modules implement IModule and are registered with ModuleRegistry.

Recommended flow:
1. Register modules
2. Resolve dependency order
3. Load all modules with ModuleLoader

## Dependency Rules
- Modules declare dependencies with minimum versions
- Cycles are rejected
- Missing or incompatible dependencies raise errors

## Hot-Load Guidance
The module system supports hot-load where safe. Modules should avoid retaining global mutable state that cannot be refreshed.

## Content Pack Integration
Content packs can declare a list of module names in pack.json. The runtime can use this to:
- Ensure required modules are loaded
- Disable packs that rely on unavailable modules

## Developer-Facing Guidance
- Prefer data packs for content-only changes
- Use modules for new systems, behaviors, or integrations
- Keep module APIs stable and documented
