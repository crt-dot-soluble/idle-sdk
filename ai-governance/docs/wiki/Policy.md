# Policy Model

## File
- governance.config.json (preferred)

## Schema
- schemas/governance.schema.json

## Required Keys
- version
- policyGeneratedBy
- bootstrap
- versionControl
- testing
- documentation
- language
- autonomy
- phases
- ciCdEnforced
- remoteRequired

## Key Options
### versionControl
- git-local
- git-remote
- git-remote-ci

### testing
- full
- baseline

### documentation
- inline
- comments-only
- generate

### autonomy
- feature
- milestone
- fully-autonomous

## Behavioral Effects
- The policy overrides all defaults once generated.
- Phase enforcement and stop contracts derive from policy.
- Tasks and scripts read the policy to determine required steps.

## Updates
Policy changes must be executed via the VS Code tasks to ensure deterministic auditability.
