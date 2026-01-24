# Governance Overview

## Purpose
This framework governs AI-assisted development with a machine-first, policy-driven model.
VS Code is the mandatory human interface.

## Core Principles
1. Governance before code
2. Specification is law
3. Defaults apply until bootstrap overrides them
4. Bootstrap is the only override mechanism
5. Phased, incremental delivery
6. Explicit stop & review contracts
7. CI/CD is the enforcer
8. Agents are accountable actors
9. Autonomy must be declared
10. Nothing is done without policy compliance
11. All bounded decisions use VS Code Tasks

## Components at a Glance
The system is composed of:
- Policy and governance instructions
- Deterministic agent roles
- VS Code tasks and scripts
- Specification, plans, and ledgers
- Templates and documentation

See [Components](Components.md) for the full inventory.

## Phased Workflow Model
Phases are mandatory and sequential:
1. Phase 0: Governance Bootstrap
2. Phase 1: Specification & Architecture
3. Phase 2: Scaffolding
4. Phase 3: Core Implementation
5. Phase 4: Testing & Hardening
6. Phase 5: Documentation & Release

See [Workflow](Workflow.md) for details and expected artifacts per phase.

## Immutability (Downstream Projects)
- Governance files are editable ONLY in this repository.
- In downstream projects, AI-governance data is immutable once bootstrap generates policy.
- Policy changes must be executed via VS Code Tasks.
