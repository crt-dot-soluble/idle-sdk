# Global AI Development Instructions

These rules apply to ALL agents.

---

## Policy First
- Load governance policy before acting.
- If no policy exists, run the bootstrap task to generate it.
- Policy overrides specification defaults once generated.

---

## VS Code Task Requirement
- Any bounded decision (governance, enforcement, workflow, autonomy, scope) MUST be made through a VS Code Task.
- When input is required or a selection is based on predefined options, use VS Code native pickers/confirmations.
- Text input is reserved for large code payloads or explicit human intervention due to roadblocks.

---

## Governance Immutability (Downstream Projects)
- Governance files are editable ONLY in this repository.
- In downstream projects, AI-governance data is immutable once bootstrap generates policy.
- Policy changes must be executed via VS Code Tasks, never by direct file edits.

---

## Spec-Driven Development
- All work begins with specifications.
- Specs define behavior, interfaces, and constraints.
- Code is derived from specs, never the reverse.
- Spec checklist (minimum):
	- Inputs/outputs and error cases
	- Non-goals and constraints
	- Test plan (unit/integration/acceptance)

---

## Template System
- Templates for root-level NAME.md files live under /templates and are the source of truth.
- Use /templates/PLAN.md for planning documents under /plans.
- Use /templates/SPECIFICATION.md for implementation specs under /spec.

---

## Modularity & Architecture
- Systems are modular and API-first.
- Each module has a single responsibility.
- Internal implementation details are not relied upon externally.

---

## Testing & Validation
- Every feature requires tests.
- Tests define expected behavior.
- Failing tests block progress.

---

## Version Control
- Commit frequently.
- One logical change per commit.
- Commits must leave the system buildable and testable.
- All git, GitHub, branch, merge, and commit operations are owned by the GIT agent.
- Bug detection, debugging, and bugfixing are owned by the DEBUG agent.

---

## Logging & Memory
- Significant decisions must be logged.
- Prefer append-only ledgers.
- Preserve long-term context.

---

## Self-Correction Loop
Generate → Test → Diagnose → Fix → Re-test → Commit

---

## Execution Continuity
- Continue autonomously until the specified end result is deliverable.
- Do not request permission at each minor step; instead report each completed step.
- Obey the stop contract defined in the governance policy.
- If no stopping point/end goal is specified, stop and request a clear end point before proceeding.

---

## Agent Declaration Enforcement
- State the current agent mode on every response.
- If switching modes, declare the switch before any task execution or analysis.
