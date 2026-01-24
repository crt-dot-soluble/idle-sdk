# MEMORY-LEDGER
# Append-only immutable memory log.
# Record significant decisions, actions, and results.

- <entry>
- 2026-01-24: Converted narrative engine spec into machine-first specification at spec/SPECIFICATION.md with explicit requirements, interfaces, architecture, and acceptance criteria.
- 2026-01-24: Created Phase 1 plan at plans/PHASE-1-ARCHITECTURE-PLAN.md; logged open questions for renderer, persistence, and plugin packaging.
- 2026-01-24: Architecture decisions recorded: renderer = WebAssembly (C# → WASM + TS/JS renderer), persistence = SQLite, plugin packaging = NuGet package.
- 2026-01-24: Created .NET solution idle-sdk.sln with core library project and xUnit test project (scaffolding).
- 2026-01-24: Implemented engine spine components (SimulationClock, TickScheduler, EventHub, Module system) with xUnit coverage; tests pass.
- 2026-01-24: Added snapshot persistence interfaces and SQLite-backed store with tests.
- 2026-01-24: Implemented JSON schema data pack validation with NJsonSchema and tests.
- 2026-01-24: Implemented action system core (registry, runner, definitions) with tests.
- 2026-01-24: Implemented skill system core (registry, XP curve, progression) with tests.
- 2026-01-24: Implemented combat system core (tick-based resolution) with tests.
- 2026-01-24: Implemented world/universe hierarchy core and registry with tests.
- 2026-01-24: Implemented multi-currency wallet system with registry and tests.
- 2026-01-24: Implemented item registry and inventory system with tests.
- 2026-01-24: Implemented equipment loadout system with bonuses and tests.
- 2026-01-24: Implemented scene layer model and diff engine with tests.
- 2026-01-24: Implemented achievements and collections core systems with tests.
- 2026-01-24: Implemented audio system core with registry and mixer.
- 2026-01-24: Implemented quest system core with evented progress updates and tests.
- 2026-01-24: Implemented hotkey manager and theme system core with tests.
- 2026-01-24: Implemented developer sandbox core with runtime inspector and tests.
