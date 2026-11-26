---
layout: default
title: "Plan 002: Pooling, High Score & Music Player"
permalink: /plans/002-pooling-highscore-and-music-player
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

# Plan 002: Obstacle Pooling, High Score Persistence & Music Player

- **Status**: Proposed (WIP / Todo)
- **Date**: 2026-11-26
- **Target Branch**: `feat/music-pooling-highscore` *(branching from `main` after `update/unity-6` merge)*

---

## Goals & Scope

Following the Unity 6 (`6000.0.85f2`) upgrade and DoomFire optimization in [Plan 001](001-unity6-migration-and-doomfire-optimization.md), this milestone focuses on gameplay runtime polish, score persistence, and showcasing original music:

1. **Obstacle Object Pooling** — Eliminate runtime `Instantiate`/`Destroy` allocations during gameplay and retries.
2. **Persistent High Score (`PlayerPrefs`)** — Wire both score panels in `GameOverCanvas` and persist best score across sessions.
3. **Music System (`Aestial/Unity-OpenMusicPlayer`)** — Integrate an event-driven soundtrack player with non-intrusive song notifications and Main Menu playback controls to showcase the colleague's original score.
4. **Targeted UX & Audio Polish** — Guard gameplay taps against UI clicks and expose shared audio/mute UX.

> **Deferred**: [`HealthMeter.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Player/HealthMeter.cs) / health bar UI remains intentionally deferred to a future plan.

---

## 1. Obstacle Object Pooling

### Current State
- [`Pool.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Models/Pool.cs) already implements a circular `GameObject[]` pool (`GetCurrent`, `GetNext`, `Reset`, `DestroyAll`), but is unused.
- [`ObstacleSpawner.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/ObstacleSpawner.cs) calls `Instantiate(obstaclePrefab, transform)` every `spawnInterval` and `Destroy(child.gameObject)` in `OnDisable()`.
- [`LeftLeaveCameraDestroy.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/LeftLeaveCameraDestroy.cs) calls `Destroy(gameObject)` when an obstacle exits the left camera border.
- [`GapDistance.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/GapDistance.cs) only invokes `SetDistance()` in `Start()`, which will not re-apply on subsequent pooled activations if `gapDistance` changes.

### Implementation Plan
1. **Enhance [`Pool.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Models/Pool.cs)**:
   - Update `Reset()` (or add `ReturnAll()`) to `SetActive(false)` on every pooled `GameObject` and reset `currentIndex`.
   - Ensure `GetNext()` safely skips or warns if pool size is too small for active on-screen obstacles (recommended default `poolSize = 6`–`8` for `spawnInterval = 2s`, `speed = -2`).
2. **Update [`ObstacleSpawner.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/ObstacleSpawner.cs)**:
   - Initialize `Pool` once in `Awake()` (not `OnEnable()`) so retries reuse the same pre-warmed obstacles.
   - In `Spawn()`, pull from `pool.GetNext()`, set `localPosition = new Vector2(0, height)`, configure `GapDistance.distance` + call `SetDistance()`, and call `SetActive(true)`.
   - In `OnDisable()`, replace `DestroyAll()` with `pool?.Reset()` so obstacles deactivate cleanly on `GameManager.Reset()` without destroying the pool.
   - Destroy pool items only in `OnDestroy()`.
3. **Replace Destroy-on-Exit in [`LeftLeaveCameraDestroy.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/LeftLeaveCameraDestroy.cs)**:
   - Support disabling (`gameObject.SetActive(false)`) when leaving the left border (or add a `disableInsteadOfDestroy` flag defaulting to `true` on [`Obstacle.prefab`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Prefabs/Obstacle.prefab)) so pooled obstacles recycle cleanly.
4. **Update [`GapDistance.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/GapDistance.cs)**:
   - Call `SetDistance()` in `OnEnable()` (or explicitly from `ObstacleSpawner.Spawn()`) and guard `Awake()`/`Start()` initialization order on `BoxCollider2DSize` so re-enabled obstacles always match `gapDistance`.

---

## 2. Persistent High Score (`PlayerPrefs`)

### Current State
- [`Game.unity`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scenes/Game.unity) `GameOverCanvas` already contains two score panels:
  - `Score_Panel` → `Score_Text (TMP)` (currently wired to `GameManager.highScoreText`, displaying current `score` on Game Over).
  - `High Score_Panel (01)` → `Score_Text (TMP)` (currently unwired and stuck at `"0"`).
- [`GameManager.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/GameManager.cs) does not read or save `PlayerPrefs`.

### Implementation Plan
1. **Separate Run Score vs. Best Score in [`GameManager.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/GameManager.cs)**:
   - Rename/add clear serialized references:
     - `scoreText` (`ScoreCanvas` live HUD counter)
     - `finalScoreText` (`GameOverCanvas` → `Score_Panel` → `Score_Text (TMP)`)
     - `highScoreText` (`GameOverCanvas` → `High Score_Panel (01)` → `Score_Text (TMP)`)
     - Optional `newBestBadge` (`GameObject` / TMP label shown when the player sets a new record).
   - Store and load `const string HighScoreKey = "HighScore";` via `PlayerPrefs.GetInt(HighScoreKey, 0)` on `Awake()`/`Start()`.
   - In `GameOver()`: compare `score > bestScore`; if higher, update `bestScore = score`, `PlayerPrefs.SetInt(HighScoreKey, bestScore)`, and `PlayerPrefs.Save()`.
2. **Wire [`Game.unity`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scenes/Game.unity)**:
   - Re-bind `finalScoreText` and `highScoreText` in `GameManager` Inspector so both `Score_Panel` and `High Score_Panel (01)` display accurate values.

---

## 3. Music System via `Aestial/Unity-OpenMusicPlayer`

### Package Overview
- **Repository**: [Aestial/Unity-OpenMusicPlayer](https://github.com/Aestial/Unity-OpenMusicPlayer) (`dev.oxido.openmusicplayer`, v`0.1.9`)
- **Core Features**: Event-driven `MusicPlayerCore`, `MusicPlaylistScriptableObject` (`Create → Audio → Music Playlist`), loop/shuffle/sequential playback modes, volume control, `HUD Samples`, and `Singleton Player` (`DontDestroyOnLoad`).

### Unity 6 Compatibility Check (First Step)
- `OpenMusicPlayer`'s `package.json` currently declares `"com.unity.textmeshpro": "3.0.9"` and `"com.unity.ugui": "1.0.0"`, whereas Flappy Burger on Unity 6 uses `"com.unity.ugui": "2.0.0"` (which absorbed TextMesh Pro).
- **Action**: Test adding `https://github.com/Aestial/Unity-OpenMusicPlayer.git` via Unity Package Manager first; if UPM reports a conflict on `com.unity.textmeshpro`, update `Unity-OpenMusicPlayer` (or fork/branch) for Unity 6 UGUI `2.0.0` + `FindFirstObjectByType`/`FindObjectsByType` compatibility.

### Adapted UX Design for Portrait Arcade Play
Rather than crowding the 9:16 playfield with a full desktop music bar, adapt `OpenMusicPlayer` to showcase the colleague's tracks cleanly:

1. **Persistent Singleton Player (`Main.unity` → `Game.unity`)**:
   - Initialize the `Singleton Player` (`MusicPlayerCore` + `DontDestroyOnLoad`) in [`Main.unity`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scenes/Main.unity) so music continues uninterrupted across scene loads (`Main` ↔ `Game`) and retries (`GameManager.Reset()`).
   - Configure a `MusicPlaylistScriptableObject` asset (e.g., `Assets/Audio/FlappyBurger_OST.asset`) containing the colleague's tracks with proper **Title** and **Artist/Composer** metadata.
   - Verify `OpenMusicPlayer` playback/timers keep working cleanly when `Time.timeScale == 0` during Game Over (or intentionally duck volume slightly on Game Over and restore on Retry).

2. **Main Menu Controls (`Main.unity`)**:
   - Add a compact, skin-themed music strip (e.g., bottom or top-corner widget on `MenuCanvas` / `SkinsCanvas`):
     - **Play / Pause** toggle, **Previous / Next Track**, and **Mute / Volume** button.
     - Current track + composer credit readout using `GrapeSoda SDF` font.
     - Attach [`SkinImage`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Skins/SkinImage.cs) to all music UI buttons so they automatically match **Heaven**, **Darkness**, and **Inferno** themes.
   - Persist music mute / volume state in `PlayerPrefs` (`"MusicVolume"` / `"MusicMuted"`).

3. **Song Popup Notification ("Now Playing" Toast)**:
   - Hook into `MusicPlayerCore`'s track-changed event (using the package's popup/notification feature or a lightweight slide-in banner driven by its events) to briefly display:
     > ♪ *Now Playing: `<Song Title>` — `<Composer>`*
   - In `Game.unity`, place the toast in a non-blocking top/side overlay (`Raycast Target = false`) below or beside the score counter so it **never intercepts taps** or obscures pipes/player.

---

## 4. Additional Optimization & UX Opportunities

1. **Prevent UI Clicks from Triggering Flap (`PlayerInput.cs`)**:
   - [`PlayerInput.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Player/PlayerInput.cs) currently fires `onAction` on any `Input.GetMouseButtonDown(0)` or `TouchPhase.Began`. If any clickable UI (pause/mute/music button) is visible in `Game.unity`, tapping it would also jump/start the run.
   - **Fix**: Guard pointer/touch input with `EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(...)` before invoking `onAction`.
2. **Audio & WebGL Autoplay Considerations**:
   - WebGL browsers block audio until the first user gesture. Ensure `MusicPlayerCore` starts or resumes gracefully on the first tap/click if WebGL suspended the `AudioContext`.
   - Organize audio assets into `Assets/Audio/Music/` and `Assets/Audio/SFX/` (or keep `Assets/Sounds/` cleanly separated) and consider an `AudioMixer` for independent Music vs. SFX volume control.
3. **Cache `Camera.main` & Physics2D Checks**:
   - Replace `Camera.main` calls where cached references suffice and double-check `ProjectSettings/DynamicsManager` / `Physics2DSettings` if needed.

---

## Open Decisions

- [ ] **OpenMusicPlayer Installation**: Add directly via Git UPM URL (`https://github.com/Aestial/Unity-OpenMusicPlayer.git`), or update `Unity-OpenMusicPlayer`'s `package.json` for Unity 6 (`com.unity.ugui@2.0.0`) first?
- [ ] **In-Game Music Controls**: Keep playback controls only in `Main.unity` (with just the "Now Playing" popup toast during gameplay), or also add a small Mute/Skip button in `Game.unity` / `GameOverCanvas`?
- [ ] **Game Over Music Behavior**: Keep music playing at normal volume on Game Over, or smoothly duck volume (e.g., `50%`) until Retry/Exit?
- [ ] **Track Metadata**: Confirm track titles, composer credit string, and default playback mode (Sequential vs. Shuffle, Loop All).

---

## Checklist

- [ ] Verify / resolve `Unity-OpenMusicPlayer` (`dev.oxido.openmusicplayer`) Unity 6 compatibility and add to `Packages/manifest.json`
- [ ] Refactor [`Pool.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Models/Pool.cs), [`ObstacleSpawner.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/ObstacleSpawner.cs), [`GapDistance.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/GapDistance.cs), and [`LeftLeaveCameraDestroy.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Obstacles/LeftLeaveCameraDestroy.cs) for zero-destroy pooling
- [ ] Implement `PlayerPrefs` `"HighScore"` persistence in [`GameManager.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/GameManager.cs) and wire `High Score_Panel (01)` in [`Game.unity`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scenes/Game.unity)
- [ ] Add `MusicPlaylistScriptableObject`, `Singleton Player`, skin-themed Main Menu controls, and non-blocking "Now Playing" popup notification
- [ ] Guard [`PlayerInput.cs`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scripts/Player/PlayerInput.cs) against UI raycast clicks
- [ ] Test across Unity Editor, WebGL (autoplay + retry loop), and Android
- [ ] Update `docs/` and `README.md` before opening PR to `main`
