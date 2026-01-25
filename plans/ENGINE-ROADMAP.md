# Engine Roadmap (Machine-First)

> Feature-focused roadmap for idle-sdk engine expansion and hardening.

## Title
Engine Feature Roadmap — idle-sdk

## Problem Statement
Define the ordered feature roadmap needed to harden and expand the engine for production and demo-grade usage without introducing game-specific logic.

## Context & Assumptions
- Current state: Core engine systems and Avalonia demo are functional.
- Constraints (legal, security, compliance, budget, time): Governance mandates phased delivery, full test coverage, inline documentation, and CI/CD.
- Assumptions (explicit and testable): The engine remains game-agnostic; UI is a demo shell only.

## Goals
- Deliver a hardened, deterministic, modular engine with reliable persistence and data packs.
- Expand gameplay systems with configurable, data-driven modules and hooks.

## Non-Goals
- Shipping a complete game or custom art assets.
- Multiplayer/networked gameplay.

## Stakeholders
- Owner: Engine maintainers
- Reviewers: Architecture and gameplay system reviewers
- Consumers/Users: Game teams, modders, tooling integrators

## Scope Boundaries
- Included components/modules: Engine core, data packs, persistence, gameplay systems, demo wiring.
- Excluded components/modules: Dedicated game content and proprietary art.

## Requirements Traceability
- Spec reference(s): [spec/SPECIFICATION.md](spec/SPECIFICATION.md)
- Policy constraints: Full test coverage, inline documentation, CI/CD enforcement, phased delivery.

## Phases & Milestones
- Phase A: Hardening & Determinism
  - Scope: Save/load, replay, validation, performance metrics.
  - Artifacts: deterministic replay harness, persistence integration, data pack loader.
  - Exit criteria: replay tests pass, snapshot load/save stable, validation errors surfaced.
- Phase B: Action/Skill/Combat Expansion
  - Scope: modifiers, cooldowns, status effects, AI behaviors, multi-enemy encounters.
  - Exit criteria: configurable action pipeline and combat behaviors with test coverage.
- Phase C: Economy & Crafting Expansion
  - Scope: sinks/sources, exchanges, pricing rules, crafting queues, production chains.
  - Exit criteria: economy rules configurable, crafting chain tests.
- Phase D: World & Scene Evolution
  - Scope: exploration state, environmental modifiers, day/night, node effects.
  - Exit criteria: world state persists and renders consistent scene diffs.
- Phase E: Meta Systems & Collections
  - Scope: achievement rule engine, bestiary/compendium growth, titles/badges.
  - Exit criteria: metadata systems configurable and queryable.
- Phase F: Tooling & Modding
  - Scope: schema generation, mod packaging, sandbox scripting hooks.
  - Exit criteria: tooling pipeline documented and validated.
- Milestones:
  - M1: Deterministic save/replay complete.
  - M2: Expanded action/combat pipeline.
  - M3: Economy/crafting chain complete.
  - M4: World/scene modifiers integrated.
  - M5: Meta systems finalized.
  - M6: Tooling/modding pipeline ready.

## Work Breakdown
- Tasks (ordered):
	1. Implement snapshot save/load flow in demo and engine interfaces.
	2. Add deterministic replay harness and golden tests.
	3. Build data pack loader with schema validation and error reporting.
	4. Expand action pipeline (cooldowns, inputs/outputs, modifiers).
	5. Expand combat (status effects, AI scripts, multi-enemy).
	6. Add economy sinks/sources and exchange rule engine.
	7. Expand crafting/production queues.
	8. Add world exploration state and environmental modifiers.
	9. Implement meta rules engine for achievements/collections.
	10. Provide mod packaging and schema tooling.
- Dependencies:
	- Schema validation and persistence required before replay harness.
- Risks & mitigations:
	- Risk: Feature creep slows core stability.
		Mitigation: Enforce phase exit criteria with tests.

## Quality & Testing
- Test strategy: deterministic unit/integration + replay fixtures.
- Required test types: unit, integration, acceptance.
- Coverage expectations: 100% per policy.

## Delivery Plan
- Timeline: iterative by phase, per milestone gates.
- Rollout/Migration steps: schema versioning with migration hooks.
- Backout plan: snapshot restore and module disable.
- Acceptance checkpoints: per phase exit criteria.

## Documentation Plan
- Required docs: engine API docs, data pack schema docs, modding guides.
- Update targets: README, wiki, spec, demo instructions.

## Open Questions
- Confirm tooling scope for schema generation (CLI vs in-app).
- Define official mod package format and signature requirements.
