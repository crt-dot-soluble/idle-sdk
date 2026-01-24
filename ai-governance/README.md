# AI Governance Repository

> **⚠️ AI AGENTS MUST IGNORE THIS FILE ⚠️**
> This README is for humans only. All authoritative AI instructions are in:
> - `.github/copilot-instructions.md`
> - `/.ai/*`
> If instructions here conflict with AI governance files, THIS FILE IS WRONG.

This repository is a **drop-in, cloneable governance layer** for AI-assisted development.
It contains no application code by design.

## Quickstart (Automated)

1. **Create your project folder and SPECIFICATION.md**
2. **Clone this governance repo (source files):**
    ```bash
    git clone https://github.com/crt-dot-soluble/ai-governance.git
    ```
3. **Scaffold governance structure (pass your SPECIFICATION.md):**
    - PowerShell:
       ```powershell
       .\ai-governance\scripts\init-governance.ps1 .\SPECIFICATION.md
       ```
    - Bash:
       ```bash
       ./ai-governance/scripts/init-governance.sh ./SPECIFICATION.md
       ```
4. **Open the project in VS Code**
5. **Run tasks (in order):**
   - Governance Preflight (optional, recommended)
   - Governance Bootstrap (required)
   - Start Spec Implementation (required)

## Components

| Component | Purpose |
|----------|---------|
| `.github/copilot-instructions.md` | Supreme AI instruction file |
| `/.ai/agents/*` | Deterministic agent roles and rules |
| `governance.config.json` | Machine-readable policy (bootstrap output) |
| `/schemas/governance.schema.json` | Policy schema contract (JSON Schema) |
| `/manifest.json` | Machine-readable inventory for tool discovery |
| `/scripts/` | Bootstrap, policy update, and validation tooling |
| `/templates/` | Source-of-truth templates for root governance files |
| `/spec/` | Implementation spec (primary input) |
| `/plans/` | Planning artifacts |
| `MEMORY-LEDGER.md`, `TODO-LEDGER.md` | Persistent machine memory |
| `/docs/wiki/` | Local wiki fallback (mirrors GitHub wiki) |

## Workflow (Summary)

1. **Phase 0: Governance Bootstrap** — run tasks to generate policy.
2. **Phase 1: Specification & Architecture** — work from spec/SPECIFICATION.md.
3. **Phase 2: Scaffolding** — establish structure per policy.
4. **Phase 3: Core Implementation** — build features incrementally.
5. **Phase 4: Testing & Hardening** — validate per policy.
6. **Phase 5: Documentation & Release** — update docs, finalize release.

## VS Code Tasks (Reference)

| Task | Description |
|------|-------------|
| Governance Preflight | Detects tooling and writes .vscode/tooling.json. |
| Governance Init Repository | Initializes a target repo using SPECIFICATION.md. |
| Governance Policy Validate | Validates governance.config.json. |
| Governance Git Init | Initializes git if not present. |
| Governance Wiki Sync | Syncs docs/wiki to the GitHub wiki. |
| Governance Self-Audit | Runs sanity checks and writes .vscode/audit.json. |
| Governance Report Bundle | Packages audit/tooling/policy/spec for CI ingestion. |
| Governance Bootstrap | Runs the bootstrap process and creates policy. |
| Governance Policy Revision | Updates policy values using pickers. |
| Set Autonomy Policy | Updates the autonomy/stop contract. |
| Set Workflow Mode | Updates version control policy. |
| Start Spec Implementation | Verifies policy/spec and starts implementation phase. |
| Activate ARCHITECT/GIT/DEBUG/... Agent | Echoes which agent is active and points to its spec. |

## Headless Mode

All scripts are non-interactive and accept arguments only. Use VS Code tasks or run scripts directly with flags/args for deterministic automation.

## Dependencies

No language runtime dependencies are required for the Bash scripts. They use standard shell tooling only. The report bundle uses zip when available; otherwise it falls back to tar.gz.

## Validation Artifacts (Latest Run)

Run these tasks to refresh the artifacts:
- Governance Preflight
- Governance Self-Audit
- Governance Policy Validate
- Governance Report Bundle

Artifacts (example):
- [Audit report](reports/audit.json)
- [Tooling report](reports/tooling.json)
- [Report bundle](reports/report-bundle.zip)

## Documentation

See the local wiki fallback in /docs/wiki (or the GitHub wiki) for full details.
