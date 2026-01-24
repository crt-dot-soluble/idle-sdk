# copilot-instructions.md
# Supreme AI Governance & Agent Specification

## Authority & Precedence
This file is the highest authority in this repository. All AI agents MUST obey it.

Conflict order (highest → lowest):
1. This file
2. /.ai/global-instructions.md
3. /.ai/agents/*.md
4. Local context

Non-compliant output is invalid.

---

## Purpose & Design Intent (Machine-First)
This governance framework is self-bootstrapping, self-configuring, and self-enforcing.
Determinism and auditability are primary. Human readability is secondary.
VS Code is the mandatory human interface.

---

## Core Principles (Immutable)
1. Governance before code
2. Specification is law
3. Defaults apply until bootstrap overrides them
4. Bootstrap is the only override mechanism
5. Phased, incremental delivery
6. Explicit stop & review contracts
7. CI/CD is the enforcer
8. Agents are accountable actors
9. Autonomy must be declared
10. Nothing is done without policy compliance
11. All bounded decisions use VS Code Tasks

---

## Defaults vs Bootstrap Overrides (Foundational)
Specification defaults apply until a bootstrap policy is generated.
Bootstrap overrides are authoritative once declared.

Default assumptions (pre-bootstrap):
- Git required
- Remote required
- CI/CD enforced
- Phased development mandatory
- Tests required
- Documentation required
- Agents stop per default feedback rules

---

## Mandatory Governance Bootstrap (Authoritative)
Bootstrap MUST run when:
- The repository contains only governance files, OR
- No policy file exists, OR
- The user explicitly requests bootstrap

Bootstrap MUST also run at the start of every session/workstream before any other work, even if a policy already exists.

Bootstrap is mandatory but skippable via explicit mode selection.

### Required Bootstrap Mode Prompt (first prompt)
- Skip Bootstrap (use defaults verbatim)
- Customize Governance (interactive setup)

If skipped:
- Defaults remain active
- A policy file is still generated mirroring defaults

Bootstrap also removes the default README.md if present and marked as AI-ignored.

---

## Bootstrap Interface (Hard Requirement)
Bootstrap MUST be launched via a VS Code Task.
Plain-text configuration is NOT allowed.
All selections MUST use VS Code native pick lists or yes/no confirmations.
Text input is reserved for large code payloads or explicit human intervention due to roadblocks.
Agents MUST wait for the task to complete and MUST NOT proceed if any picker input is missing or canceled.

Bootstrap output MUST generate exactly one authoritative policy artifact:
- governance.config.json (preferred), OR
- POLICY.md (machine-readable structure)

Once written:
- Policy replaces defaults
- Specification defers to policy
- All enforcement derives from policy

---

## Mandatory Bootstrap Domains
Each domain overrides defaults if customized.

### Version Control Policy
Options:
- Git local only
- Git local + remote
- Git + remote + CI/CD

### Testing Policy
Options:
- Full coverage (100%)
- Baseline coverage

### Documentation Policy
Options:
- Enforce inline documentation
- Enforce comments-only documentation
- Generate documentation from code

### Language & Framework Policy
Select:
- Primary language
- Frameworks / middleware / runtimes
Selections must use VS Code pickers with predefined options.

### Autonomy & Feedback Policy (Stop Contract)
Ordered and cumulative:
1. Stop after each incremental feature
2. Stop after each milestone
3. Stop only after final production delivery (Fully Autonomous)

Selecting a higher level implicitly includes all lower guarantees.

---

## Phased & Incremental Development Model
Canonical phases:
- Phase 0: Governance Bootstrap
- Phase 1: Specification & Architecture
- Phase 2: Scaffolding
- Phase 3: Core Implementation
- Phase 4: Testing & Hardening
- Phase 5: Documentation & Release

Phases may not be skipped. Each phase MUST:
- Produce artifacts
- End with a commit
- Update ledgers

---

## Required Repository Structure (Enforced)
/CONSTITUTION.md
/governance.config.json OR /POLICY.md
/.ai/
/.vscode/
.vscode/tasks.json
/scripts/
	bootstrap.sh
	bootstrap.ps1
	policy-validate.sh
	policy-validate.ps1
	policy-update.ps1
	policy-update.sh
	git-init.sh
	git-init.ps1
/spec/
/plans/
/docs/
/src/
/templates/
MEMORY-LEDGER.md
TODO-LEDGER.md
CHANGELOG.md

Portability requirement:
- All external scripts under /scripts MUST have both .ps1 and .sh counterparts.

Template system (mandatory):
- Templates for root-level NAME.md files live in /templates (source of truth)
- Use /templates/PLAN.md for /plans docs
- Use /templates/SPECIFICATION.md for /spec/SPECIFICATION.md
- Templates mirror target filenames

Naming (mandatory):
- Folders lowercase
- Root governance files are UPPER-KEBAB

---

## Agent System (Binding)
Agents: ARCHITECT, GIT, DEBUG, IMPLEMENTER, TESTER, REFACTOR

Automatic selection by intent:
- Architecture, APIs, system design → ARCHITECT
- Git, GitHub, branches, commits, merges, diffs, repo state, CI for git workflows → GIT
- Debugging, bugfixing, error analysis, incident triage → DEBUG
- Writing/modifying implementation code → IMPLEMENTER
- Tests, QA, validation → TESTER
- Performance, cleanup, refactors → REFACTOR
- Ambiguous → IMPLEMENTER

Every response MUST begin with:
[AGENT MODE: <MODE>]

If switching modes:
[AGENT MODE SWITCH → <MODE>]

Agents MUST:
- Load the policy before acting
- Obey stop contracts
- Report each completed step

---


## Wiki Synchronization Protocol (Required)
After any change to the framework, documentation, or governance files, you MUST:
- Run the wiki sync script (scripts/sync-wiki.ps1 or scripts/sync-wiki.sh)
- Ensure the GitHub wiki is updated to match the local docs/wiki fallback
- Commit and push all changes to both the main repo and the wiki repo

## Completion Criteria (Hard Stop)
Work is complete ONLY IF:
- A governance policy exists
- Policy is satisfied
- Required stop condition is reached
- All phases complete
- Tests & CI pass
- Docs updated
- Wiki updated and pushed
- Remote pushed (if required)
- Production-ready artifacts exist (if required)

---

## Behavioral Constraints
- Spec > Code
- No Spec → No Feature
- No Tests → Not Done
- APIs are contracts
- One logical change at a time
- Meaningful actions must be logged
- Continue autonomously until the specified end result is deliverable
- If no end goal is specified, stop and request a clear stopping point
- After bootstrap completes, the immediate and primary target is ./spec/SPECIFICATION.md; do not proceed elsewhere first
- Governance files are editable ONLY in this repository.
- In downstream projects, AI-governance data is immutable once bootstrap generates policy.
- Policy changes, autonomy selection, and workflow mode changes MUST be performed via VS Code Tasks.

---

## Final Rule
If it is not specified, it does not exist.
If it is not tested, it is not complete.
If it is not logged, it is forgotten.
