# Bootstrap Process

## Trigger Conditions
Bootstrap MUST run when:
- The repository contains only governance files, OR
- No governance policy file exists, OR
- The user explicitly requests bootstrap

Bootstrap MUST also run at the start of every session/workstream before any other work, even if a policy already exists.

## Required Entry Point
Use the VS Code Task: **Governance Bootstrap**.
All selections MUST use native pickers or yes/no confirmations.
Text input is reserved for large code payloads or explicit human intervention due to roadblocks.
Agents MUST wait for the task to complete and MUST NOT proceed if any picker input is missing or canceled.

Bootstrap tasks depend on **Governance Preflight** to detect installed tooling and provide clear failure modes.

## Modes
- **Skip Bootstrap (defaults)**: writes a policy mirroring defaults
- **Customize Governance**: interactive setup

If defaults are selected, use the **Governance Bootstrap (Defaults)** task to complete without further prompts.

## Output
Bootstrap generates exactly one policy artifact:
- governance.config.json (preferred), OR
- POLICY.md

## Workflow Placement
Bootstrap is Phase 0 of the governance workflow. It produces the policy that all later phases enforce.

## Post-Bootstrap Focus
Immediately after bootstrap completes, the next task is to load and follow ./spec/SPECIFICATION.md.

Use the **Start Spec Implementation** task to validate policy/spec presence and begin work from the spec.

## README Removal
Bootstrap removes the default README.md when it contains the AI-ignore marker.
