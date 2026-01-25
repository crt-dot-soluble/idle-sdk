# idle-sdk

⚠️ **IMPORTANT — HUMAN READERS ONLY** ⚠️
This README is the only human‑focused document. It must not be reviewed, included in AI context, or recognized by AI.

**AI AGENTS MUST IGNORE THIS FILE.**
Authoritative AI behavior is defined exclusively in:
- .github/copilot-instructions.md
- /.ai/*

If instructions here conflict with AI governance files, THIS FILE IS WRONG.

---

## One‑Line Summary
An engine‑first .NET 10 idle RPG SDK with deterministic simulation, modular systems, and no‑code content packs.

## Overview
idle-sdk is a modular, deterministic engine for building idle and semi‑idle RPGs. It provides core systems (actions, skills, combat, world, economy, items) plus a sandbox, persistence, and tooling to support rapid iteration. A minimal Avalonia demo app is included.

## Why This Exists
Most idle RPG implementations hard‑code rules and content. idle-sdk separates engine logic from data so teams can ship new content quickly, hot‑reload packs, and maintain deterministic behavior.

## Key Features
- Deterministic simulation loop with offline reconciliation.
- Modular systems for actions, skills, combat, world, economy, inventory, equipment, quests, and achievements.
- JSON content packs that can be enabled/disabled and hot‑reloaded without code.
- Sandbox tools for inspection and safe runtime mutation.

## Architecture at a Glance
- Primary language: C#
- Framework/runtime: .NET 10
- UI stack (demo): Avalonia

## Getting Started (Humans)
### Prerequisites
- .NET SDK 10

### Setup
1. Clone the repository
2. Restore dependencies
3. Run tests

### Run Demo
dotnet run --project src/idle-sdk.demo/IdleSdk.Demo.csproj

### Test
dotnet test idle-sdk.sln

## Content Packs (No‑Code)
Content packs are JSON‑only bundles:
- pack.json (manifest)
- data/*.json (actions, skills, items, worlds, quests, etc.)

Packs can be enabled/disabled and hot‑reloaded at runtime. No code changes required.

## Logs
- Local app logs: %LocalAppData%\idle-sdk-demo\logs\demo.log
- Workspace mirror: logs\demo.log

## Usage
- Use the core systems in src/idle-sdk.core
- Add JSON content packs for new skills, quests, items, areas, and combat profiles
- Optionally extend via plugins for advanced logic

## Roadmap
See plans/ENGINE-ROADMAP.md

## Contributing
See docs/README.md

## License
Unspecified

---

## Governance Notice (Human‑Facing)
This repository is governed by an AI governance framework.
Governance behavior is authoritative in .github/copilot-instructions.md and /.ai/*.

Templates in this folder mirror required filenames in the repo:
- CHANGELOG.md
- CONSTITUTION.md
- MEMORY-LEDGER.md
- TODO-LEDGER.md
- PLAN.md
- SPECIFICATION.md
