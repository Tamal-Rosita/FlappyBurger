---
layout: default
title: Overview
---

<nav class="doc-nav">
  <a href="{{ '/' | relative_url }}">Overview</a>
  <a href="{{ '/architecture' | relative_url }}">Architecture</a>
  <a href="{{ '/getting-started' | relative_url }}">Setup &amp; Builds</a>
  <a href="{{ '/privacy' | relative_url }}">Privacy</a>
  <a href="{{ '/terms' | relative_url }}">Terms</a>
</nav>

# Flappy Burger (*Flappy BACON-A-TOR*)

**Flappy Burger** (branded in-game as **Flappy BACON-A-TOR**) is a portrait-mode 2D arcade game built with **Unity 6 (`6000.0.85f2`)** by **Tamal Rosita**. Tap, click, or press Space to keep a winged burger aloft through scrolling cave pipes, earn points, and switch between three unlock-free visual themes.

![Flappy BACON-A-TOR Cover]({{ '/assets/images/FlappyBACON-A-TOR_cover.png' | relative_url }})

---

## Project Snapshot

| Property | Value |
| --- | --- |
| **Product Name** | Flappy Burger (`Flappy BACON-A-TOR`) |
| **Studio / Company** | Tamal Rosita |
| **Current Version** | `0.2.1` (Android `versionCode: 101`) |
| **Unity Version** | `6000.0.85f2` (Unity 6) |
| **Target Platforms** | Android, iOS, WebGL (PWA), Standalone |
| **Orientation** | Portrait (`360x640` WebGL default) |
| **Build Scenes** | `Main.unity` (Menu) &rarr; `Game.unity` (Gameplay) |

---

## Gameplay & Controls

- **One-button arcade loop**: Tap the screen, left-click, or press `Space` to start the run and flap upward.
- **Scoring**: Passing through the center trigger between each top and bottom pipe increments the live score counter.
- **Game Over**: Colliding with pipes, the ceiling, or the lower hazard triggers a randomized explosion sound effect, freezes time (`Time.timeScale = 0`), and opens the Game Over overlay with **AGAIN** (retry) and **EXIT** (return to main menu).
- **DoomFire Effect**: The gameplay scene renders a fixed-grid retro Doom-style fire strip (`DoomFireRenderer` + `Custom/DoomFire` shader) along the bottom hazard collider in a single GPU draw call.

---

## Visual Themes (Skins)

Players can switch themes in the **Skins** menu (`Main.unity`). The selected theme is saved locally via `PlayerPrefs` (`"Skin"`) and applied across the background sprite, background tint, camera clear color, and UI button colors.

<div class="screenshot-grid">
  <figure>
    <img src="{{ '/assets/images/Baconator-Heaven.png' | relative_url }}" alt="Heaven Skin">
    <figcaption><strong>Heaven</strong> — Bright sky &amp; pastel buttons</figcaption>
  </figure>
  <figure>
    <img src="{{ '/assets/images/Baconator-Darkness.png' | relative_url }}" alt="Darkness Skin">
    <figcaption><strong>Darkness</strong> — Teal cavern &amp; neon magenta UI</figcaption>
  </figure>
  <figure>
    <img src="{{ '/assets/images/Baconator-Hell.png' | relative_url }}" alt="Inferno Skin">
    <figcaption><strong>Inferno</strong> — Crimson underworld &amp; purple UI</figcaption>
  </figure>
</div>

---

## Feature Status & Roadmap

### Implemented
- Portrait-oriented Main Menu (`Main.unity`) with animated idle burger, game title logo, and live `Application.version` label
- Skin selector (`Heaven`, `Darkness`, `Inferno`) persisted with `PlayerPrefs`
- Tap / mouse / keyboard (`Space`) flap controls with custom gravity kinematics
- Scrolling parallax-style background shader (`Custom/Sprite Scrolling`)
- Procedural pipe spawning with Inspector-tunable gap distance, spawn rate, and vertical variation
- Live score HUD and Game Over retry/exit flow
- Randomized flap and explosion sound effects
- Hybrid fixed-grid DoomFire hazard strip (`DoomFireRenderer.cs` + `Custom/DoomFire` shader)
- Android, iOS, and WebGL PWA icon & build assets

### In Progress / Roadmap
- **Health Meter (`HealthMeter.cs`)**: Component is attached to `Player` in `Game.unity` (`maxHealth = 3`), awaiting UI bar hookup and `onDeath` wiring
- **Persistent High Score**: `GameManager` currently shows the current run's score in `highScoreText` on Game Over; `PlayerPrefs` best-score tracking is planned
- **Object Pooling (`Pool.cs`)**: Generic pool helper exists in `Assets/Scripts/Models/Pool.cs` to replace `Instantiate`/`Destroy` in `ObstacleSpawner`
- **Dedicated Flight & Death Animations**: Expand `SpriteAnimation` beyond frame reset on jump

---

## Documentation Pages

- [**Technical Architecture**]({{ '/architecture' | relative_url }}) — Scene hierarchy, script reference, event wiring, shaders, and asset breakdown.
- [**Setup & Builds**]({{ '/getting-started' | relative_url }}) — Unity 6 setup, OpenUPM packages, Inspector tuning, adding skins, and platform builds.
- [**Privacy Policy**]({{ '/privacy' | relative_url }}) &bull; [**Terms & Conditions**]({{ '/terms' | relative_url }}) — Published store legal pages.
