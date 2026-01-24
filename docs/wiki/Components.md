# Components

This page describes the major components of the governance system and how they fit together.

## Governance & Policy
- **Supreme instructions:** .github/copilot-instructions.md
- **Agent roles:** /.ai/agents/*
- **Policy output:** governance.config.json (or POLICY.md if configured)
- **Policy schema:** /schemas/governance.schema.json
- **Manifest:** /manifest.json

## Workflow Inputs
- **Primary contract:** /spec/SPECIFICATION.md
- **Planning artifacts:** /plans/

## Operations
- **VS Code tasks:** /.vscode/tasks.json
- **Scripts:** /scripts/ (bootstrap, policy update, validation, sync)

## Memory & Audit Trail
- **MEMORY-LEDGER.md** — persistent machine memory
- **TODO-LEDGER.md** — active work backlog
- **CHANGELOG.md** — release and change tracking

## Templates
- **/templates/** — source-of-truth templates for root governance files

## Documentation
- **/docs/wiki/** — local wiki fallback for the GitHub wiki
