---
layout: default
title: Development Plans
permalink: /plans/
---

<nav class="doc-nav">
  <a href="{{ '/' | relative_url }}">Overview</a>
  <a href="{{ '/architecture' | relative_url }}">Architecture</a>
  <a href="{{ '/getting-started' | relative_url }}">Setup &amp; Builds</a>
  <a href="{{ '/plans/' | relative_url }}">Plans</a>
  <a href="{{ '/privacy' | relative_url }}">Privacy</a>
  <a href="{{ '/terms' | relative_url }}">Terms</a>
  <a class="nav-cta" href="https://tamalrosita.itch.io/flappy-bacon-a-tor" target="_blank" rel="noopener">Play on itch.io &rarr;</a>
</nav>

# Development Plans & Decision Log

Internal engineering plans, audits, and architecture decision records for **Flappy Burger (*Flappy BACON-A-TOR*)**.

Keeping plans in `docs/plans/` (outside `Assets/`):
- Prevents Unity from importing Markdown files or generating `.meta` files.
- Keeps the repository root focused on standard Unity folders (`Assets/`, `Packages/`, `ProjectSettings/`).
- Publishes plans cleanly on the project's documentation site (`/plans/`) and keeps them readable alongside code in Git.

## Index

| ID | Date | Title | Branch | Status |
| --- | --- | --- | --- | --- |
| [001](001-unity6-migration-and-doomfire-optimization.md) | 2026-11-26 | Unity 6 Migration Audit & DoomFire Optimization Plan | `update/unity-6` | Completed |
| [002](002-pooling-highscore-and-music-player.md) | 2026-11-26 | Obstacle Pooling, High Score Persistence & OpenMusicPlayer | *(next branch)* | Proposed (WIP / Todo) |

## Naming & Workflow Convention

1. Create each new plan as `NNN-short-kebab-title.md` (e.g., `002-health-meter-and-highscore.md`).
2. Track status at the top of each file: `Proposed` → `Approved` → `In Progress` → `Completed`.
3. Record open questions/decisions directly inside the plan so choices stay documented alongside the code changes.
