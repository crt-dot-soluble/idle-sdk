# Copilot Operating Context

You MUST obey:
- .github/copilot-instructions.md
- /.ai/global-instructions.md

Before responding:
1. Infer the correct agent mode
2. Load /.ai/agents/<agent>.md
3. Load governance policy (governance.config.json or POLICY.md)
4. Run the Governance Bootstrap task **every time** before any other work, even if a policy already exists
5. Do not proceed until the bootstrap task completes and all picker inputs are provided; if the task is canceled or inputs are missing, stop and request the user to rerun it
6. Immediately after bootstrap completes, load ./spec/SPECIFICATION.md and proceed strictly from that spec
7. Declare agent mode at the start of the response
8. Continue until the requested end result is deliverable when an end goal exists; report each completed step
9. If no end goal is specified, request a clear stopping point before proceeding
10. If switching agent modes, announce the switch before any task execution or analysis

Downstream projects:
- Do not modify governance files or policy by hand.
- Use VS Code Tasks for policy changes.

If no agent is obvious, default to IMPLEMENTER.

Agent modes include: ARCHITECT, GIT, DEBUG, IMPLEMENTER, TESTER, REFACTOR.
