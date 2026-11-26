# Plan 001: Unity 6 Migration Audit & DoomFire Optimization

- **Status**: Implemented (Pending Testing & PR)
- **Date**: 2026-11-26
- **Branch**: `update/unity-6` → `main`

---

## Part 1: Unity 6 (`6000.0.85f2`) Migration Verification

Comparing `main` (`2022.3.21f1`) against `update/unity-6` (`6000.0.85f2`), the Unity Editor upgrade completed **cleanly with no compiler, shader, or scene serialization errors**.

### Verified Healthy
- **Editor & Packages**: [`ProjectSettings/ProjectVersion.txt`](../ProjectSettings/ProjectVersion.txt) is updated to `6000.0.85f2`. [`Packages/manifest.json`](../Packages/manifest.json) cleanly applies Unity 6's consolidation of `com.unity.textmeshpro` into `com.unity.ugui` (`2.0.0`), upgrades 2D packages (`2d.animation` `10.2.4`, `2d.spriteshape` `10.1.1`), removes deprecated `com.unity.ide.vscode`, and updates OpenUPM dependencies (`naughtyattributes` `2.1.6`, `playerprefseditor` `1.2.5`).
- **Scenes & Assets**: [`Main.unity`](../Assets/Scenes/Main.unity), [`Game.unity`](../Assets/Scenes/Game.unity), [`GrapeSoda SDF.asset`](../Assets/Fonts/GrapeSoda%20SDF.asset), and [`Background_Sprite Scrolling.mat`](../Assets/Materials/Background_Sprite%20Scrolling.mat) are properly re-serialized for Unity 6 (`Rigidbody2D` `m_LinearDamping`, `CircleCollider2D` v3, TMP `m_TextWrappingMode`).
- **Logs & Shaders**: No C# assembly errors (`LogAssemblyErrors (0ms)`) and no shader compiler warnings or errors in `Logs/shadercompiler-UnityShaderCompiler-*.log`. Render pipeline remains Built-in RP (`m_CustomRenderPipeline: {fileID: 0}` in [`ProjectSettings/GraphicsSettings.asset`](../ProjectSettings/GraphicsSettings.asset)).

### Minor Pre-PR Cleanup Items (Recommended Before Merging to `main`)
1. **Replace obsolete API in [`SkinSystem.cs`](../Assets/Scripts/Skins/SkinSystem.cs#L23)**: `FindObjectsOfType<SkinImage>()` is marked obsolete in Unity 6 — update to `FindObjectsByType<SkinImage>(FindObjectsSortMode.None)`.
2. **Remove backup image artifact**: Delete [`Press/FlappyBACON-A-TOR_cover.png~`](../Press/FlappyBACON-A-TOR_cover.png~) (editor backup file) if unintended.
3. **Optional `UserSettings/` untrack**: `UserSettings/Layouts/default-6000.dwlt` currently shows noisy layout diffs in `git status`. Consider adding `UserSettings/` to [`.gitignore`](../.gitignore) and untracking it (`git rm -r --cached UserSettings/`).
4. **Commit documentation & `.gitignore` updates**: Commit [`docs/`](../docs), [`README.md`](../README.md), [`.gitignore`](../.gitignore), untracked `Logs/`, and [`plans/`](README.md).

---

## Part 2: Why `ScreenFire` Lags on Mobile & WebGL

Inspecting [`ScreenFire.cs`](../Assets/DoomFire/ScreenFire.cs) and the `Fire` GameObject in [`Game.unity`](../Assets/Scenes/Game.unity):

1. **Resolution-Dependent Cell Explosion**:
   - `Game.unity` sets `tileSize = 5` and `size = 5`.
   - `ScreenFire.Start()` computes `width = (int)(Screen.width / tileSize)` and `height = (int)(Screen.height / tileSize)` across the **entire screen**.
   - On a `1440x3200` mobile device, that creates `288 x 640 = 184,320` cells (vs. `72 x 128 = 9,216` on `360x640` WebGL).
2. **`OnGUI()` Draw Call Bottleneck**:
   - Unity invokes `OnGUI()` **multiple times per frame** (Layout, Repaint, Input events).
   - Each call iterates over every cell, evaluates `gradient.Evaluate(...)` on the CPU, and submits a separate `GUI.DrawTexture(...)` immediate-mode draw call — up to **hundreds of thousands of draw calls per frame**, even for transparent cells above the flames and even while `Time.timeScale == 0` during Game Over.
3. **Propagation & Sizing Quirks**:
   - `CalculateFirePropagation()` does not clamp `newFireIntensity` to `[0, 1]` and can write across row boundaries / near buffer ends.
   - `Start()` caches `Screen.width`/`Screen.height` once and never adapts to viewport resize, and `OnGUI()` cannot participate in 2D `SortingLayer` ordering behind UI canvases.

---

## Part 3: Optimization Plan — Fixed-Grid Hybrid CPU + GPU Shader

Keep the classic retro Doom fire cellular automaton while reducing rendering from **~184,000 `OnGUI` calls** down to **1 GPU draw call** at a **fixed logical pixel resolution**.

### Architecture Split

```text
[CPU - Fixed Grid @ ~13 Hz]                   [GPU - Built-in RP Shader @ Every Frame]
  byte[gridWidth * gridHeight]                  Custom/DoomFire.shader on 1 Quad / Mesh
  (e.g., 96 x 32 = 3,072 cells)                 ├── Samples R8 intensity texture (Point filter)
  └──► Upload to R8 Texture2D                   ├── Looks up color in 256x1 Gradient LUT texture
       (only when simulation ticks)             └── Optional per-cell tile mask (Circle/Square)
```

### Step-by-Step Implementation Plan

#### 1. Replace `Screen.width`/`Screen.height` with Fixed Logical Grid Dimensions
- In a refactored fire component (e.g., `DoomFireEffect.cs`, replacing `OnGUI` in [`ScreenFire.cs`](../Assets/DoomFire/ScreenFire.cs) while keeping [`Assets/DoomFire/Scenes/`](../Assets/DoomFire/Scenes) intact if needed):
  - Expose fixed `gridWidth` (e.g., `96`–`128`) and `gridHeight` (e.g., `32`–`48`) instead of dividing `Screen.width`/`Screen.height` by `tileSize`.
  - Optionally support `autoAspectWidth` (derive `gridWidth = Mathf.RoundToInt(gridHeight * aspect)` clamped to a safe max like `160`) so aspect ratios never stretch pixels while keeping cell count bounded (`<= 7,680` cells regardless of device DPI).
  - Position/scale the fire quad strictly over the **bottom hazard strip** (matching `Fire`'s `BoxCollider2D` at `y = -5.6` in `Game.unity`) rather than simulating empty transparent space across the full screen height.

#### 2. Optimize CPU Propagation & Eliminate `Gradient.Evaluate` Per Cell
- Store fire intensities in a compact `byte[]` buffer (`0..255`, where `255` = hottest bottom row, `0` = extinguished).
- Run propagation only when `time >= updateFrequency` (`0.075s`), and **skip simulation ticks when `Time.timeScale == 0`**.
- Fix row-wrapping/clamping in `CalculateFirePropagation()`:
  - Keep wind/horizontal jitter within `[0, gridWidth - 1]` and clamp decayed intensity to `[0, 255]`.
- Upload intensities into a small `Texture2D(gridWidth, gridHeight, TextureFormat.R8, mipChain: false)` with `FilterMode.Point` and `TextureWrapMode.Clamp` using `GetRawTextureData<byte>()` + `Apply(false, false)`.
  - Zero per-tick allocations and zero per-pixel `Gradient.Evaluate` calls on the CPU.

#### 3. Bake `Gradient` into a `256x1` LUT Texture & Render via Custom Shader
- Bake the Inspector `Gradient` once in `Start()` / `OnValidate()` into a `256x1` `Texture2D` (`TextureFormat.RGBA32`, `FilterMode.Bilinear`, `TextureWrapMode.Clamp`).
- Create `Assets/Materials/DoomFire.shader` (`Custom/DoomFire`, Built-in RP transparent unlit shader matching [`SpriteScrolling.shader`](../Assets/Materials/SpriteScrolling.shader) conventions):
  - Samples `_MainTex` (the `R8` simulation texture) with `tex2D(_MainTex, i.uv).r`.
  - Maps intensity `r` through `_PaletteTex` (`tex2D(_PaletteTex, float2(r, 0.5))`).
  - Optional `_TileTex` (`Circle.png` / `Square.png` / `escama.png`): tiles across `frac(i.uv * float2(_GridWidth, _GridHeight))` in the fragment shader so retro circular/scaled fire particles render in **1 single draw call**.

#### 4. Scene Integration in [`Game.unity`](../Assets/Scenes/Game.unity)
- Attach the shader material to a world-space quad / `SpriteRenderer` / `RawImage` on `Fire` (using the `Environment` or `Player` sorting layer so `ScoreCanvas` and `GameOverCanvas` render cleanly in front of the flames).
- Remove `OnGUI()` completely from the gameplay path.
- Verify behavior across:
  - Unity Editor (`Game` view at `360x640`, `1080x1920`, `1440x3200`)
  - Pause / Game Over (`Time.timeScale == 0`)
  - WebGL & Android builds (Profiler check: expect **1 draw call** and `< 0.1 ms` CPU per simulation tick).

---

## Open Decisions (Resolved)

- [x] **Fire Coverage Area**: Cover only the bottom hazard strip (matching `Fire`'s `BoxCollider2D` bounds).
- [x] **Cell Visual Style**: Render pure crisp retro pixel cells (`FilterMode.Point`), no tile masks needed.
- [x] **Pre-PR Repo Cleanup**: Keep `UserSettings/` tracked for cross-workstation editor configs; `Press/FlappyBACON-A-TOR_cover.png~` removed manually.

---

## Pre-PR Verification Checklist

- [x] Apply `FindObjectsByType` fix in [`SkinSystem.cs`](../Assets/Scripts/Skins/SkinSystem.cs#L23)
- [x] Implement fixed-grid `R8` + LUT shader DoomFire replacement ([`DoomFireRenderer.cs`](../Assets/DoomFire/DoomFireRenderer.cs), [`DoomFire.shader`](../Assets/Materials/DoomFire.shader), [`DoomFire.mat`](../Assets/Materials/DoomFire.mat), [`Game.unity`](../Assets/Scenes/Game.unity))
- [x] Cleanup confirmation (`Press/FlappyBACON-A-TOR_cover.png~` removed; `UserSettings/` retained)
- [ ] Test `Main.unity` and `Game.unity` in Editor + WebGL/Android build
- [ ] Commit changes and open PR from `update/unity-6` → `main`
