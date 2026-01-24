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
