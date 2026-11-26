---
layout: default
title: Setup & Builds
permalink: /getting-started
---

<nav class="doc-nav">
  <a href="{{ '/' | relative_url }}">Overview</a>
  <a href="{{ '/architecture' | relative_url }}">Architecture</a>
  <a href="{{ '/getting-started' | relative_url }}">Setup &amp; Builds</a>
  <a href="{{ '/privacy' | relative_url }}">Privacy</a>
  <a href="{{ '/terms' | relative_url }}">Terms</a>
</nav>

# Setup, Customization & Builds

## Prerequisites

- **Unity Editor**: `6000.0.85f2` (Unity 6)
- **External Dependencies** (automatically resolved via OpenUPM in `Packages/manifest.json`):
  - `com.dbrizov.naughtyattributes` (`2.1.6`) — provides `[Button]` Inspector controls on obstacle helpers
  - `com.bgtools.playerprefseditor` (`1.2.5`) — editor window for inspecting/resetting `PlayerPrefs` (`"Skin"`)
- **Unity Modules**: 2D (`animation`, `pixel-perfect`, `sprite`, `spriteshape`, `tilemap`), `ugui` (TextMesh Pro), `recorder`, `timeline`, `test-framework`

---

## Running Locally

1. Clone the repository:
   ```bash
   git clone git@github.com:Tamal-Rosita/FlappyBurger.git
   ```
2. Open the project folder in **Unity Hub** using **Unity `6000.0.85f2`**.
3. Open `Assets/Scenes/Main.unity` to start from the title & skin menu, or `Assets/Scenes/Game.unity` to test gameplay directly.
4. Set the Game view aspect ratio to a **portrait** resolution (e.g., `9:16` or `360x640`) for accurate framing.

---

## Tuning Gameplay in the Inspector

Most gameplay values are exposed directly on scene components and prefabs:

| Parameter | Location | Current Value |
| --- | --- | --- |
| Gravity | `Game.unity` &rarr; `Player` &rarr; `GravityMovement.gravity` | `-9` |
| Jump Force | `Game.unity` &rarr; `Player` &rarr; `GravityMovement.jumpForce` | `4.5` |
| Spawn Interval | `Game.unity` &rarr; `ObstacleSpawner.spawnInterval` | `2` s |
| Gap Distance | `Game.unity` &rarr; `ObstacleSpawner.gapDistance` | `3.5` |
| Obstacle Speed | `Assets/Prefabs/Obstacle.prefab` &rarr; `MoveOnAxis2D.speed` | `-2` |
| Fire Update Rate / Grid | `Game.unity` &rarr; `Fire` &rarr; `DoomFireRenderer` | `0.075` s / `16` rows |

In Edit Mode, you can preview pipe spacing on `Obstacle.prefab` using the **SetDistance** and **SetSize** NaughtyAttributes buttons on `GapDistance`, `ChildrenDistance2D`, and `BoxCollider2DSize`.

---

## Adding a New Skin Theme

1. In the Project window, right-click inside `Assets/Skins/` and choose **Create &rarr; Skin Theme**.
2. Assign a seamless background `Sprite` (e.g., from `Assets/Sprites/`), `backgroundColor`, `buttonsColor`, and `cameraColor`.
3. Add the new `.asset` to the `skins` array on the `SkinSystem` GameObject in both `Main.unity` and `Game.unity`.
4. Add a UI button in `SkinsCanvas` (`Main.unity`) calling `SkinSystem.SetSkin(index)` with the new array index.

---

## Platform & Build Configuration

- **Android**:
  - Package Name: `com.tamalrosita.flappyburger`
  - Min SDK: `23` &bull; Target SDK: `34` &bull; Version Code: `101`
  - Launcher icons and store graphics live in `Assets/Icons/android/`
- **iOS**:
  - Bundle ID: `com.Aestial.FlappyBurger` &bull; Target OS: `13.0`
  - AppIcon set lives in `Assets/Icons/ios/`
- **WebGL (PWA)**:
  - Default resolution: `360x640` (`Minimal` WebGL template)
  - PWA icons and manifest snippet live in `Assets/Icons/web/`
  - Packaged build archive: `Builds/FlappyBACON-A-TOR_v0.2.1_web.zip` *(ignored by Git)*

---

## Repository Layout

```text
FlappyBurger/
├── Assets/
│   ├── DoomFire/         # CPU DoomFire ScreenFire effect & demo scenes
│   ├── Fonts/            # GrapeSoda font & TMP SDF asset
│   ├── Icons/            # Android, iOS, and WebGL/PWA icon sets
│   ├── Materials/        # SpriteScrolling shader & scrolling materials
│   ├── Prefabs/          # Obstacle.prefab
│   ├── Scenes/           # Main.unity, Game.unity
│   ├── Scripts/          # GameManager, HealthMeter, Player, Obstacles, Skins, Sounds, UI
│   ├── Skins/            # Heaven, Darkness, Inferno ScriptableObject themes
│   ├── Sounds/           # Flap & explosion WAV SFX
│   └── Sprites/          # Winged burger, pipes, seamless backgrounds, studio logos
├── Packages/             # Unity package manifest & OpenUPM lockfile
├── Press/                # Promotional cover art
├── ProjectSettings/      # Unity 6 project, build, input, and tag settings
├── Screenshots/          # Gameplay screenshots for each skin theme
└── docs/                 # GitHub Pages documentation & legal pages
```
