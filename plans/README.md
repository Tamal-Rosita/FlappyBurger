# Development Plans & Decision Log

Internal engineering plans, audits, and architecture decision records for **Flappy Burger (*Flappy BACON-A-TOR*)**.

Keeping plans in a root-level `plans/` directory (outside `Assets/` and `docs/`):
- Prevents Unity from importing Markdown files or creating `.meta` files.
- Keeps the public GitHub Pages site (`docs/`) focused on game documentation and legal pages.
- Tracks every plan, decision, and completion status in Git so you can read and review them directly on GitHub.

## Index

| ID | Date | Title | Branch | Status |
| --- | --- | --- | --- | --- |
| [001](001-unity6-migration-and-doomfire-optimization.md) | 2026-11-26 | Unity 6 Migration Audit & DoomFire Optimization Plan | `update/unity-6` | Proposed (Awaiting Decisions) |

## Naming & Workflow Convention

1. Create each new plan as `NNN-short-kebab-title.md` (e.g., `002-health-meter-and-highscore.md`).
2. Track status at the top of each file: `Proposed` → `Approved` → `In Progress` → `Completed`.
3. Record open questions/decisions directly inside the plan so choices stay documented alongside the code changes.
