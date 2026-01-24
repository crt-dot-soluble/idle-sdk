# Repository Structure

This repository enforces a fixed structure so governance is deterministic and audit-friendly.

## Required Layout
- /CONSTITUTION.md
- /governance.config.json or /POLICY.md
- /.ai/
- /.vscode/
- /scripts/
- /spec/
- /plans/
- /docs/
- /src/
- /templates/
- /schemas/
- MEMORY-LEDGER.md
- TODO-LEDGER.md
- CHANGELOG.md
- manifest.json

## Key Components

### Governance & Policy
- .github/copilot-instructions.md — supreme instruction file
- /.ai/ — agent roles and shared rules
- governance.config.json — machine-readable policy (bootstrap output)
- schemas/governance.schema.json — policy schema contract
- manifest.json — machine-readable inventory

### Process Inputs
- /spec/SPECIFICATION.md — primary contract for work
- /plans/ — planning artifacts and decision records

### Operations & Tasks
- /.vscode/tasks.json — task entry points
- /scripts/ — bootstrap, policy update, validation, and sync scripts

### Memory & Audit
- MEMORY-LEDGER.md — durable machine memory
- TODO-LEDGER.md — ongoing work ledger
- CHANGELOG.md — versioned changes

### Templates & Docs
- /templates/ — authoritative templates for root governance files
- /docs/wiki/ — local wiki fallback for GitHub wiki

## Portability
All scripts under /scripts must have both .ps1 and .sh counterparts.

## Initialization
Use the repository initializer CLI to scaffold this structure in downstream projects without copying this repository's .git data.
