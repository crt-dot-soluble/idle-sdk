# CHANGELOG
# Machine-readable release history.

## 0.1.0
- Initial AI governance scaffold.

## 0.1.1
- Added GIT and DEBUG agent roles with governance updates.

## 0.1.2
- Clarified continuous execution behavior and stopping-point requirements.

## 0.1.3
- Enforced consistent agent declaration and pre-task switch announcements.

## 0.1.4
- Added spec template and minimum spec checks.
- Added work plan format guidance for step reporting.

## 0.1.5
- Renamed /specs to /plans for planning docs.
- Added /spec for implementation specs with a template and CI checks.

## 0.1.6
- Added /templates for root NAME.md templates and plan/spec templates.
- Standardized naming rules and updated CI/README to reflect template usage.

## 0.1.7
- Standardized implementation spec to /spec/SPECIFICATION.md and aligned template/CI checks.

## 0.2.0
- Upgraded governance spec to machine-first bootstrap/policy model.
- Added constitution, policy file, scripts, and repo structure enforcement.
- Added governance bootstrap VS Code task and policy validation in CI.

## 0.2.1
- Added policy update tasks and downstream immutability rules.
- Added policy update script and CI validation for bootstrap/policy scripts.

## 0.2.2
- Enforced VS Code native pickers for bounded inputs.
- Added predefined language and framework selections to tasks.

## 0.2.3
- Normalized "None" framework selection to an empty list in bootstrap/policy updates.

## 0.2.4
- Added missing .ps1/.sh counterparts for policy update/validate and git init scripts.

## 0.2.5
- Specified script portability requirement for .ps1/.sh counterparts.

## 0.2.6
- Bootstrap now removes the default README.md when present.

## 0.2.7
- Added repository wiki content and local fallback under /docs/wiki.

## 0.2.8
- Expanded plan/spec templates to be robust and self-describing.

## 0.2.9
- Expanded human README template for standardized project structure.

## 0.3.0
- Enforced always-on bootstrap workflow with explicit task-completion waiting.
- Added repository initializer CLI scripts (PowerShell/Bash) for downstream scaffolding.
- Clarified post-bootstrap focus on /spec/SPECIFICATION.md and updated wiki docs.
- Hardened bootstrap input validation and fixed bash bootstrap root resolution.

## 0.3.1
- Added tooling preflight detection and cross-platform task execution.
- Added Python availability checks for shell scripts that require Python.

## 0.3.2
- Made policy shell scripts resilient to non-root working directories.

## 0.3.3
- Added spec existence checks during bootstrap and a spec-start task/scripts.
