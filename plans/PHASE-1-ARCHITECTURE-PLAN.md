# Plan — Phase 1 Specification & Architecture

> This plan defines the execution details for Phase 1 per governance requirements.

## Title
Phase 1: Specification & Architecture for idle-sdk Engine Core

## Problem Statement
The engine requires a machine-first, testable specification and an explicit architecture definition to drive downstream implementation without ambiguity.

## Context & Assumptions
- Current state: Narrative specification exists; no implementation code has begun.
- Constraints (legal, security, compliance, budget, time): Governance policy mandates full test coverage, inline documentation, CI/CD, and phased delivery.
- Assumptions (explicit and testable): The engine targets .NET 10 and must support desktop-first and web-compatible rendering via an abstraction layer.

## Goals
- Deliver a machine-first implementation specification under [spec/SPECIFICATION.md](spec/SPECIFICATION.md).
- Define component boundaries, interfaces, and acceptance criteria sufficient to begin Phase 2 scaffolding.

## Non-Goals
- Implementing engine code or game-specific logic.

## Stakeholders
- Owner: Engine maintainers
- Reviewers: Architecture reviewers
- Consumers/Users: Gameplay implementers and plugin authors

## Scope Boundaries
- Included components/modules: Simulation loop, event bus, module system, data pack schema, persistence, sandbox, core system boundaries.
- Excluded components/modules: Actual renderer implementation, game-specific content, networking.

## Requirements Traceability
- Spec reference(s): [spec/SPECIFICATION.md](spec/SPECIFICATION.md)
- Policy constraints: Full test coverage, inline documentation, CI/CD enforcement, phased delivery.

## Phases & Milestones
- Phase 1: Specification & Architecture; artifact(s): machine-first spec and plan; exit criteria: spec complete, ledgers updated.
- Phase 2: Scaffolding; artifact(s): solution structure, module stubs.
- Phase 3: Core Implementation; artifact(s): engine spine systems.
- Milestones: Phase 1 sign-off enables scaffolding start.

## Work Breakdown
- Tasks (ordered):
	1. Convert narrative spec into machine-first template.
	2. Define architecture components and interfaces.
	3. Update ledgers and changelog.
- Dependencies:
	- Governance policy must be present and active.
- Risks & mitigations:
	- Risk: Spec ambiguities block implementation.
		Mitigation: Log open questions explicitly and resolve before Phase 2.

## Quality & Testing
- Test strategy: Define test coverage expectations in spec; implement tests in Phase 4.
- Required test types (unit/integration/acceptance): Unit, integration, acceptance.
- Coverage expectations (if policy-defined): 100% coverage target.

## Delivery Plan
- Timeline: Phase 1 complete within current workstream.
- Rollout/Migration steps: N/A for Phase 1.
- Backout plan: Revert spec changes if rejected by review.
- Acceptance checkpoints: Spec reviewed and accepted by maintainers.

## Documentation Plan
- Required docs: Spec updated; ledger and changelog entries.
- Update targets (README, wiki, API docs): Wiki sync required after governance/doc changes.

## Open Questions
- Confirm renderer abstraction strategy before Phase 2.
- Confirm persistence backend before Phase 2.
