---
layout: default
title: Flappy BACON-A-TOR
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

<section class="hero-card">
  <div>
    <span class="hero-eyebrow">Tamal Rosita &bull; Unity 6 Arcade</span>
    <h1>Flappy <span class="hero-accent">BACON-A-TOR</span></h1>
    <p>
      Pilot a winged burger through scrolling cave pipes, dodge scorching Doom-style flames,
      and swap between three unlock-free worlds in this fast one-button Flappy Bird tribute.
    </p>
    <div class="cta-group">
      <a class="btn btn-primary" href="https://tamalrosita.itch.io/flappy-bacon-a-tor" target="_blank" rel="noopener">
        Play Now! (itch.io)
      </a>
      <a class="btn btn-secondary" href="{{ '/architecture' | relative_url }}">
        Technical Docs
      </a>
      <a class="btn btn-secondary" href="https://github.com/Tamal-Rosita/FlappyBurger" target="_blank" rel="noopener">
        GitHub Repo
      </a>
    </div>
  </div>
  <div class="hero-media">
    <img src="{{ '/assets/images/press/FlappyBACON-A-TOR_cover.png' | relative_url }}" alt="Flappy BACON-A-TOR Cover">
  </div>
</section>

## Why Flappy Burger?

<div class="feature-grid">
  <div class="feature-card">
    <h3>One-Tap Controls</h3>
    <p>Tap on mobile, left-click, or press <code>Space</code> to start your run and flap upward with crisp arcade gravity.</p>
  </div>
  <div class="feature-card">
    <h3>3 Switchable Worlds</h3>
    <p>Choose between <strong>Heaven</strong>, <strong>Darkness</strong>, and <strong>Inferno</strong> themes—saved automatically in <code>PlayerPrefs</code>.</p>
  </div>
  <div class="feature-card">
    <h3>Retro DoomFire Hazard</h3>
    <p>Fixed-grid cellular fire simulation rendered with a custom <code>Custom/DoomFire</code> LUT shader for 60 FPS mobile &amp; WebGL play.</p>
  </div>
  <div class="feature-card">
    <h3>Built in Unity 6</h3>
    <p>Clean Inspector-tunable <code>UnityEvent</code> architecture ready for Android, iOS, and WebGL PWA builds.</p>
  </div>
</div>

---

## Visual Themes (Skins)

Switch themes anytime from the **Skins** menu in [`Main.unity`](https://github.com/Tamal-Rosita/FlappyBurger/blob/main/Assets/Scenes/Main.unity). Each `SkinScriptableObject` restyles the scrolling background, camera backdrop, and UI buttons together.

<div class="screenshot-grid">
  <figure>
    <img src="{{ '/assets/images/screenshots/Baconator-Heaven.png' | relative_url }}" alt="Heaven Skin">
    <figcaption><strong>Heaven</strong> — Bright sky &amp; pastel buttons</figcaption>
  </figure>
  <figure>
    <img src="{{ '/assets/images/screenshots/Baconator-Darkness.png' | relative_url }}" alt="Darkness Skin">
    <figcaption><strong>Darkness</strong> — Teal cavern &amp; neon magenta UI</figcaption>
  </figure>
  <figure>
    <img src="{{ '/assets/images/screenshots/Baconator-Hell.png' | relative_url }}" alt="Inferno Skin">
    <figcaption><strong>Inferno</strong> — Crimson underworld &amp; purple UI</figcaption>
  </figure>
</div>

<div class="play-banner">
  <div>
    <strong>Ready to take flight?</strong>
    <p>Play the latest WebGL build directly in your browser on itch.io.</p>
  </div>
  <div class="cta-group">
    <a class="btn btn-primary" href="https://tamalrosita.itch.io/flappy-bacon-a-tor" target="_blank" rel="noopener">
      Play Now on itch.io &rarr;
    </a>
    <a class="btn btn-outline" href="{{ '/getting-started' | relative_url }}">
      Build &amp; Setup Guide
    </a>
  </div>
</div>

---

## Project Snapshot

| Property | Value |
| --- | --- |
| **Game Title** | Flappy Burger (`Flappy BACON-A-TOR`) |
| **Studio** | Tamal Rosita |
| **Play Online** | [tamalrosita.itch.io/flappy-bacon-a-tor](https://tamalrosita.itch.io/flappy-bacon-a-tor) |
| **Homepage** | [tamalrosita.com/FlappyBurger/](https://tamalrosita.com/FlappyBurger/) |
| **Current Version** | `0.2.3` (Android `versionCode: 0230`) |
| **Unity Version** | `6000.0.85f2` (Unity 6) |
| **Target Platforms** | WebGL (PWA), Android, iOS, Standalone |
| **Build Scenes** | `Main.unity` (Menu) &rarr; `Game.unity` (Gameplay) |

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
- Hybrid fixed-grid DoomFire hazard strip (`DoomFireRenderer.cs` + `Custom/DoomFire` shader) with bottom-up ignition and rightward wind drift
- Android, iOS, and WebGL PWA icon & build assets

### In Progress / Roadmap
- **Original Soundtrack & Music Player ([`OpenMusicPlayer`](https://github.com/Aestial/Unity-OpenMusicPlayer))**: Showcase original music score with persistent playback across scenes, Main Menu player controls, and "Now Playing" popup notifications
- **Object Pooling (`Pool.cs`)**: Integrate `Assets/Scripts/Models/Pool.cs` into `ObstacleSpawner` to eliminate runtime `Instantiate`/`Destroy` calls
- **Persistent High Score (`PlayerPrefs`)**: Save best score locally and wire both `Score_Panel` and `High Score_Panel (01)` on Game Over
- **Future Polish**: Health meter UI (`HealthMeter.cs` in `Assets/Scripts/Player/`) and dedicated flight & death animations

---

## Documentation & Legal

- [**Technical Architecture**]({{ '/architecture' | relative_url }}) — Scene hierarchy, script reference, event wiring, shaders, and asset breakdown.
- [**Setup & Builds**]({{ '/getting-started' | relative_url }}) — Unity 6 setup, OpenUPM packages, Inspector tuning, adding skins, and platform builds.
- [**Development Plans & Roadmap**]({{ '/plans/' | relative_url }}) — Engineering audits, optimization plans, and upcoming feature specs.
- [**Privacy Policy**]({{ '/privacy' | relative_url }}) &bull; [**Terms & Conditions**]({{ '/terms' | relative_url }}) — Published store legal pages.
