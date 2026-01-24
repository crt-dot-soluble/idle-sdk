# VS Code Tasks

All bounded decisions are made through VS Code Tasks.

## Governance Bootstrap
Runs the bootstrap process and creates the policy file.

## Governance Bootstrap (Defaults)
Runs bootstrap in defaults mode without prompting for additional options.

## Governance Preflight
Detects installed tools and writes a report to .vscode/tooling.json. Bootstrap and policy tasks depend on this preflight step.

## Governance Init Repository
Initializes a target repository using a provided SPECIFICATION.md path.

## Governance Policy Validate
Validates governance.config.json for required keys.

## Governance Git Init
Initializes git in the current repo if not already present.

## Governance Wiki Sync
Syncs docs/wiki to the GitHub wiki.

## Governance Report Bundle
Packages audit.json, tooling.json, governance.config.json, and SPECIFICATION.md for CI ingestion.
On Unix, zip is used if available; otherwise tar.gz is produced.

## Governance Self-Audit
Runs the sanity/audit checks and writes .vscode/audit.json.

## Governance Policy Revision
Updates policy values using native pickers.

## Set Autonomy Policy
Updates the autonomy/stop contract.

## Set Workflow Mode
Updates version control policy (git local/remote/ci).

## Start Spec Implementation
Verifies governance policy and spec exist, then signals that implementation can begin from spec/SPECIFICATION.md.

## Workflow Alignment
- Phase 0: Governance Bootstrap, Governance Preflight
- Phase 1+: Start Spec Implementation and any implementation tasks
- Policy Changes: Governance Policy Revision, Set Autonomy Policy, Set Workflow Mode

## Input Rules
- Use pickers for predefined options.
- Text input is reserved for large code payloads or explicit human intervention due to roadblocks.

## Headless Mode
All scripts are non-interactive and accept arguments only, enabling deterministic automation.

## Dependencies
No language runtime dependencies are required for the Bash scripts. They use standard shell tooling only.
