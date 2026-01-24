# Preflight Tooling Detection

The preflight task checks installed tooling and writes a report to .vscode/tooling.json.

## Purpose
- Provide a machine-first snapshot of available tools (git, python, node, etc.).
- Allow tasks to fail fast with clear errors when required tooling is missing.

## Workflow Placement
Preflight is the first step before bootstrap and any policy changes. It ensures the governance pipeline can be executed deterministically.

## Output
- .vscode/tooling.json

## Limitation
VS Code Tasks do not support dynamically disabling picker options based on script output without a custom extension. The preflight report is informational and used for validation and guidance.
