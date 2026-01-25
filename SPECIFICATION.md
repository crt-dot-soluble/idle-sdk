
# idle-sdk — Modular Idle RPG Engine Specification

**Engine Name:** idle-sdk
**Target:** .NET 10
**Platforms:** Desktop-first, Web-compatible
**Philosophy:** Engine-first, game-agnostic, data-driven, AI-friendly

---

## I. Purpose

`idle-sdk` is a **highly modular, extensible engine/framework** for building idle and semi-idle RPGs inspired by **Melvor Idle, IdleOn, and RuneScape**, while remaining fully engine-first and reusable.

The engine supports:

- Live deterministic simulation with **IdleOn-style scene playback**
- Deep **skills, combat, crafting, automation**
- **World / Map / Universe** construction and visualization
- **Multi-currency economy**, wallets, and root-account multi-profile support
- NFT-style **layered random generators** (characters, items, cosmetics)
- Collectibles, achievements, bestiary, compendiums
- Full **sound system**, hotkeys, theming
- Landing page, login, account creation
- Fully **spec-driven and AI-assisted development**

The first implementation game built on the engine is called **`idle-idol`**.

---

## II. Core Design Principles

1. **Engine, Not a Game**
   - No Melvor/IdleOn logic hard-coded
   - All mechanics expressed as systems, schemas, and modules

2. **Extreme Modularity**
   - Everything is a module
   - Core modules are sealed
   - All other systems are hot-loadable plugins

3. **Simulation-First**
   - Time-based, deterministic simulation
   - Offline progression supported natively

4. **Runtime Mutability**
   - Developer Sandbox always available
   - Data reloads without recompilation

5. **AI-Friendly by Design**
   - Clear interfaces, schemas, contracts
   - No logic hidden in UI layers

---

## III. High-Level Architecture

```

Root Account
└─ Profiles (Characters)
└─ Wallets / Inventories / Worlds
│
▼
┌──────────────┐
│  idle-sdk    │
│ Engine Core  │
└──────────────┘
│
┌─────────┼──────────────────────────┐
│         │                          │
▼         ▼                          ▼
Simulation Engine                Scene / Renderer

* Actions                         - Scene View (IdleOn-style)
* Skills                          - Background / Mid / Foreground
* Combat                          - Equipment-based animations
* Enemy AI                        - Drops / FX / XP visuals
* Quests / Events                 - NFT-style layered generator
* World Logic

  ```
       │
       ▼
  ```

Economy & Meta Systems

* Multi-currency wallets
* Trade / Exchange
* Collectibles
* Achievements
* Bestiary / Compendiums

  ```
       │
       ▼
  ```

Foundational Systems

* Database / Persistence
* Sound System
* Hotkey Manager
* Theme Manager
* Landing / Login / Accounts

```

---

## IV. Engine Core Systems

### 1. Game Loop & Timing
- Authoritative simulation loop
- Fixed timestep simulation
- Variable timestep rendering
- Offline time reconciliation

Hooks:
- `TickStart`
- `TickEnd`
- `SimulationElapsed`
- `RealTimeElapsed`

---

### 2. Event Bus
- Strongly-typed global event system
- Modules may emit, listen, transform events

Examples:
- `SkillXPGranted`
- `ItemDropped`
- `ActionCompleted`
- `CombatTick`
- `WorldLocationChanged`

---

### 3. Module / Plugin System
- Dynamic discovery
- Versioned dependencies
- Hot-load (where safe)

Modules may define:
- Systems
- UI Panels
- Data schemas
- Runtime-editable properties

---

## V. Simulation & Gameplay Systems

### 1. Action System
All gameplay is expressed as **Actions**.

Examples:
- `TrainSkillAction`
- `CombatAction`
- `MiningAction`
- `CraftingAction`
- `IdleAction`

Actions define:
- Duration
- Cooldowns
- Tags
- Inputs / Outputs
- Progression rules
- Visual state emissions

---

### 2. Skill System
- Fully data-driven skills
- Configurable XP curves
- Level unlock tables
- Passive effects
- Skill synergies

Skills may:
- Produce resources
- Modify actions
- Unlock zones, items, mechanics

---

### 3. Combat System
- Deterministic tick-based combat
- Equipment-driven stats
- Status effects
- Multi-enemy encounters
- Scriptable enemy AI behaviors

---

## VII. Content Packs (No-Code)

Content packs are JSON-only bundles that define new content without code changes:
- `pack.json` manifest with id, version, schema version, dependencies, and modules
- `data/*.json` files for actions, skills, items, quests, worlds, combat profiles, and more

Packs can be enabled/disabled at runtime and hot-reloaded from disk.
All packs are sandboxed and validated against schemas before activation.

---

## VI. World / Map / Universe System

- Multi-universe support
- Worlds → Regions → Zones → Nodes
- Visual + logical representation
- Exploration tracking
- Environmental modifiers

Features:
- Day/Night cycles
- Weather
- Seasons
- Rare/secret nodes
- Procedural expansion hooks

Scene view reflects world state directly.

---

## VII. Scene & Rendering System

### Core Scene View (IdleOn-style)
- Auto-play simulation visuals
- Manual override support
- Side-scroller or fixed-plane 2D

Layers:
- Background (location)
- Midground (nodes, enemies)
- Foreground (player, equipment)
- Overlay (drops, XP, FX)

---

## VIII. NFT-Style Layered Generator

Deterministic random generator (no Web3 dependency):

Used for:
- Characters
- Items
- Enemies
- Pets
- Cosmetics

Supports:
- Layer definitions
- Rarity weights
- Exclusion rules
- Seed-based determinism

Outputs:
- Flattened sprite
- JSON metadata
- Attribute breakdown

---

## IX. Economy & Account Systems

### 1. Root Account & Profiles
- One root account
- Multiple profiles (characters)
- Profile-specific worlds, inventories, wallets

### 2. Currency System
- Unlimited currencies
- Soft, premium, event-based
- Sink/source definitions
- Offline-safe

### 3. Trade & Exchange
- Global exchange / auction system
- Dynamic pricing
- Plugin-modifiable rules

---

## X. Items & Equipment Framework

- Item definitions via data
- Equipment slots
- Affixes, rarities
- Upgrade paths
- Cosmetic vs functional separation

---

## XI. Meta & Collection Systems

- Achievements & milestones
- Collectibles & sets
- Bestiary (creatures)
- Item compendium
- Location compendium
- Titles, badges, trophy rooms

---

## XII. UI & UX Systems

- Modular UI panels
- Dockable layout manager
- Saved layouts per profile
- Theme manager (colors, fonts)
- Hotkey manager (rebindable)

---

## XIII. Sound System

- Music layers (location-based)
- Skill / combat FX
- Environmental audio
- Volume & mixing controls
- Plugin-extensible

---

## XIV. Persistence & Database

- Deterministic save/load
- Snapshot-based state
- Plugin-safe serialization

Database stores:
- Accounts
- Profiles
- Inventories
- Wallets
- Worlds
- Achievements
- Collections

---

## XV. Developer Sandbox

- Runtime toggle
- Inspect all systems
- Modify properties live
- Inject items, XP, time skips
- Reload data without restart

---

## XVI. Roadmap / Feature Map

1. Engine Spine (loop, events, modules, DB)
2. Accounts & Login
3. Theme + Hotkeys
4. Scene Renderer + Generator
5. Actions & Skills
6. Combat & AI
7. World & Nodes
8. Crafting & Production
9. Economy & Trade
10. Quests & Events
11. Meta Systems
12. Sound System
13. Offline Progression
14. Tutorials
15. Seasonal Systems
16. Proof-of-Concept Game: **idle-idol**

---

## XVII. Final Notes

- `idle-sdk` remains engine-pure
- `idle-idol` contains all game-specific logic
- Designed for long-term expansion, modding, and AI-assisted content creation
