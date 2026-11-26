---
layout: default
title: Technical Architecture
permalink: /architecture
---

<nav class="doc-nav">
  <a href="{{ '/' | relative_url }}">Overview</a>
  <a href="{{ '/architecture' | relative_url }}">Architecture</a>
  <a href="{{ '/getting-started' | relative_url }}">Setup &amp; Builds</a>
  <a href="{{ '/privacy' | relative_url }}">Privacy</a>
  <a href="{{ '/terms' | relative_url }}">Terms</a>
  <a class="nav-cta" href="https://tamalrosita.itch.io/flappy-bacon-a-tor" target="_blank" rel="noopener">Play on itch.io &rarr;</a>
</nav>

# Technical Architecture

Flappy Burger uses a lightweight, Inspector-driven **UnityEvent** architecture. Components stay decoupled—input, physics, collision detection, audio, animation, and UI communicate through serialized `UnityEvent` callbacks wired directly in the scenes.

---

## Scenes

Both scenes are registered in `ProjectSettings/EditorBuildSettings.asset`:

| Index | Scene | Purpose |
| --- | --- | --- |
| `0` | `Assets/Scenes/Main.unity` | Title screen (`MenuCanvas`), theme switcher (`SkinsCanvas`), animated idle burger, version readout |
| `1` | `Assets/Scenes/Game.unity` | Gameplay loop, `GameManager`, `ObstacleSpawner`, `Player`, `ScoreCanvas`, `GameOverCanvas`, and `Fire` (`ScreenFire`) |

---

## Gameplay Event Flow

```text
[PlayerInput] (Space / Mouse0 / Touch Began)
   ├──► GameManager.GameStart()        ──► Enables GravityMovement & ObstacleSpawner (first tap only)
   └──► GravityMovement.Jump()         ──► Sets upward velocity (jumpForce = 4.5)
           └── onJump ───────────────────► SpriteAnimation.SetFrame(0) + RandomSound.PlayOneShot() (flap)

[CollisionEvents] (OnTriggerEnter2D on Player)
   ├── Tag == "Score" ── onScoreTrigger ─► GameManager.AddScore()
   └── Tag == "Enemy" ── onEnemyTrigger ─► GameManager.GameOver() + RandomSound.PlayOneShot() (explosion)
```

---

## Script Reference

### Core & State (`Assets/Scripts/`)

| Script | Key Responsibility |
| --- | --- |
| `GameManager.cs` | Manages `isPlaying`, `score`, `Time.timeScale`, `ScoreCanvas`, and `GameOverCanvas`. Calls `Reset()` in `Start()`. |
| `SceneSelection.cs` | Wraps `SceneManager.LoadScene(string)` for UI button transitions between `Main` and `Game`. |
| `ApplicationVersionTMProUI.cs` | Namespace `Metadrive.Utils`. Populates a `TextMeshProUGUI` label with `prefix + Application.version` on `Start()`. |

### Player (`Assets/Scripts/Player/`)

| Script | Key Responsibility |
| --- | --- |
| `PlayerInput.cs` | Polls `KeyCode.Space`, `Mouse0`, and `TouchPhase.Began` in `Update()` and invokes `onAction`. |
| `GravityMovement.cs` | Custom kinematic vertical movement (`gravity = -9`, `jumpForce = 4.5` in `Game.unity`). Invokes `onJump` when flapping and resets position to `Vector3.zero` via `Restart()`. |
| `CollisionEvents.cs` | `OnTriggerEnter2D` router checking collider tags `"Enemy"` (`onEnemyTrigger`) and `"Score"` (`onScoreTrigger`). |
| `HealthMeter.cs` *(WIP)* | Attached to `Player` in `Game.unity` (`maxHealth = 3`). Exposes `AddHealth(int)` and `UnityEvent` hooks (`onDeath`, `onHealthChange`, `onHealthIncrease`, `onHealthDecrease`), not yet wired to `GameManager`. |

### Obstacles (`Assets/Scripts/Obstacles/` & `Assets/Scripts/Models/`)

| Script | Key Responsibility |
| --- | --- |
| `ObstacleSpawner.cs` | Attached to `ObstacleSpawner`. On enable, invokes `Spawn()` repeatedly (`spawnInterval = 2`, `gapDistance = 3.5`) with random Y offset (`heightVariation`, `heightVariationOffset`). Destroys all spawned children on disable. |
| `GapDistance.cs` | Configures `Obstacle.prefab` on `Start()` (and via `[Button] SetDistance()` in Edit Mode): sets `ScoreTrigger` collider height and spaces `Top`/`Bottom` pipes symmetrically. |
| `ChildrenDistance2D.cs` | Positions `childrenPositive` and `childrenNegative` along `Axis2D.X` or `Axis2D.Y` at `±distance / 2`. |
| `BoxCollider2DSize.cs` | `[ExecuteInEditMode]` helper syncing a `Vector2 size` field to `BoxCollider2D.size` with an Inspector `[Button]`. |
| `MoveOnAxis2D.cs` | Translates the obstacle along `Axis2D.X` or `Axis2D.Y` (`speed = -2` on `Obstacle.prefab`). |
| `LeftLeaveCameraDestroy.cs` | Computes left world boundary from `Camera.main` and destroys obstacles after passing `leftBorder - destroyOffset`. |
| `Axis2DEnum.cs` | Defines `public enum Axis2D { X, Y }`. |
| `Pool.cs` *(Utility)* | Reusable circular `GameObject[]` pool (`GetCurrent`, `GetNext`, `Reset`, `DestroyAll`), ready for future integration into `ObstacleSpawner`. |

### Skins & Visuals (`Assets/Scripts/Skins/`, `Sprites/`, `Sounds/`, `Materials/`, `DoomFire/`)

| Script / Asset | Key Responsibility |
| --- | --- |
| `SkinScriptableObject.cs` | `ScriptableObject` (`[CreateAssetMenu(menuName = "Skin Theme")]`) storing `background` sprite, `backgroundColor`, `buttonsColor`, and `cameraColor`. |
| `SkinSystem.cs` | Loads saved skin index from `PlayerPrefs("Skin")` (or picks random/default), updates `SkinSprite`, `Camera.main.backgroundColor`, and all `SkinImage` instances in the scene. |
| `SkinSprite.cs` / `SkinImage.cs` | Wrapper components on background `SpriteRenderer` and UI `Image` buttons for batch color/sprite updates. |
| `SpriteAnimation.cs` | Cycles through a `Sprite[]` array using `InvokeRepeating`; `SetFrame(0)` restarts the flap cycle on jump. |
| `RandomSound.cs` | Plays a random `SoundFX` (`AudioClip` + `volume`) via `AudioSource.PlayOneShot`. |
| `SpriteScrolling.shader` | Custom `Custom/Sprite Scrolling` shader offsetting sprite UVs by `_Time * _ScrollSpeed`. |
| `DoomFire.shader` | Custom `Custom/DoomFire` shader sampling an `R8` intensity texture and mapping colors through a `256x1` gradient LUT in 1 draw call. |
| `DoomFireRenderer.cs` | Namespace `DoomFire`. Fixed-grid CPU Doom fire simulation (`R8` `Texture2D`, `FilterMode.Point`) with bottom-up ignition and configurable rightward wind drift rendered via `SpriteRenderer` (`ScreenFire.cs` retained for legacy demo scenes). |

---

## Tags, Layers & Assets

- **Tags (`ProjectSettings/TagManager.asset`)**: `Enemy`, `Score`
- **Sorting Layers**: `Environment` &rarr; `Default` &rarr; `Player`
- **Skin Assets (`Assets/Skins/`)**: `Heaven-Skin.asset`, `Darkness-Skin.asset`, `Inferno-Skin.asset`
- **Audio (`Assets/Sounds/`)**: `BurgerFlap.wav`, `Explosion.wav`, `Explosion2.wav`, `Explosion3.wav`
- **Typography (`Assets/Fonts/`)**: `GrapeSoda.ttf` and `GrapeSoda SDF.asset` (TextMesh Pro)
