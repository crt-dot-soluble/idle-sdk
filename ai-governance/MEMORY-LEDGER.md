# MEMORY-LEDGER
# Append-only immutable memory log.
# Record significant decisions, actions, and results.

- Repo bootstrapped with AI governance scaffold.
- Added GIT and DEBUG agent roles with updated governance files and tasks.
- Clarified continuous execution behavior and stopping-point requirement across governance files.
- Enforced consistent agent declaration and pre-task switch announcements.
- Added spec template, spec presence checks, and work plan format guidance.
- Renamed /specs to /plans, added /spec with template, and updated governance protocol/CI checks for new folder roles.
- Added /templates for root NAME.md and plan/spec templates; standardized naming rules and updated CI/README accordingly.
- Standardized implementation spec to /spec/SPECIFICATION.md and renamed template to /templates/SPECIFICATION.md.
- Upgraded supreme governance spec to include bootstrap policy model and enforcement.
- Added constitution, default policy file, scripts, docs/src placeholders, and bootstrap task.
- Added policy update script and VS Code tasks for policy revisions, autonomy, and workflow mode changes.
- Added downstream immutability rule for governance data.
- Enforced VS Code native pickers for bounded inputs and added predefined language/framework options.
- Normalized "None" framework selection to empty list in bootstrap/policy updates.
- Added .ps1/.sh counterparts for policy update/validate and git init scripts.
- Added specification rule requiring .ps1/.sh counterparts for all /scripts files.
- Added bootstrap removal of default README.md when marked AI-ignored.
- Added wiki content and a local /docs/wiki fallback.
- Expanded plan and specification templates for stronger standards.
- Expanded project README template for standardized human-facing structure.
- Enforced always-on bootstrap behavior with explicit task-completion waiting guidance.
- Added repo initializer CLI scripts for downstream scaffolding without .git data.
- Hardened bootstrap input validation and clarified post-bootstrap focus on /spec/SPECIFICATION.md.
- Added tooling preflight detection and cross-platform task support for bootstrap/policy tasks.
- Added explicit Python availability checks for shell-based governance scripts.
- Hardened policy shell scripts to resolve governance.config.json from the script location.
- Added spec existence checks during bootstrap and a spec-start task/scripts.
