# Workflow

This workflow is mandatory and sequential. Each phase produces artifacts and respects the policy.

## Phase 0: Governance Bootstrap
- Run **Governance Preflight** (recommended)
- Run **Governance Bootstrap** to generate policy
- Confirm policy output in governance.config.json

## Phase 1: Specification & Architecture
- Load /spec/SPECIFICATION.md
- Define architecture and interfaces per the spec
- Capture decisions in /plans/

## Phase 2: Scaffolding
- Create required project structure
- Use templates for root governance files
- Align repo layout with policy requirements

## Phase 3: Core Implementation
- Implement features incrementally
- Update TODO-LEDGER.md and MEMORY-LEDGER.md as work progresses
- Keep changes aligned to the spec contract

## Phase 4: Testing & Hardening
- Add or update tests per policy
- Validate against CI/CD requirements
- Fix defects before progressing

## Phase 5: Documentation & Release
- Update docs and changelog
- Ensure wiki sync is performed
- Prepare release artifacts as required

## Stop Contracts
Autonomy level is enforced by policy:
- **feature**: stop after each incremental feature
- **milestone**: stop after each milestone
- **fully-autonomous**: stop only after final production delivery
