# Pachi TODO

A rolling log of todo items.

## 2026-09-02

- [ ] expose necessary props as exports
- [ ] documentation pass

- [x] TICKET-01: Remove VisualConfig Pipeline
    - [x] Delete `src/art/VisualConfig.cs`, `src/art/VisualConfigBinding.cs`, `src/art/visual_config.tres`, `src/art/visual_showcase.tscn`, and `src/art/tests/VisualConfigTests.cs`.
    - [x] Remove all `VisualConfigBinding` fields and `ApplyVisualConfig()` calls from `Pin.cs`, `Pocket.cs`, `CardUI.cs`, `BallAwardIndicator.cs`, `PocketBallsIndicator.cs`, `Yakumono.cs`, and `EnvironmentBackground.cs`.
    - [x] Replace visual properties with standard Godot scene `[Export]` properties (`Texture2D`, `Color`) and native `.tscn` configurations.
- [x] TICKET-02: Unwire Procedural Shape Sprites
    - [x] Keep `src/shape_sprites/` in the codebase for prototyping.
    - [x] Replace scene references in `pocket.tscn`, `yakumono.tscn`, and other scenes with native Godot `Sprite2D`, `Polygon2D`, or `CollisionShape2D` (retaining circle shape sprites for pegs).
- [ ] TICKET-03: Prune Stale Documentation
    - [ ] Retain `docs/notes/20260827_jacob_playtest.md`, `CONTEXT.md`, `CODE_STYLE.md`, and `AGENTS.md`.
    - [ ] Archive or delete obsolete design specs in `docs/design/`, stale ADRs (e.g. `0001-visual-config-and-sandbox.md`), and unused scratch notes.
    - [ ] Ensure `CONTEXT.md` serves as the sole domain reference for terminology and game mechanics.
- [ ] TICKET-04: Structured Godot C# Test Framework
    - [ ] Adopt a standard Godot 4 C# test framework (e.g. `GoDotTest` + `Shouldly` or `GdUnit4Net`).
    - [ ] Migrate existing test suites (`BallStuckTests`, `BallVisualsTests`, `CardGuardrailTests`, `PocketIndicatorTests`, `PinToolPerformanceTests`) to discrete test classes using `[Test]` / `[TestCase]` with `[Setup]` / `[Cleanup]` lifecycles.
    - [ ] Split test suites into pure domain tests and scene integration tests.
    - [ ] Remove screenshot capture assertions from headless CI runs.
    - [ ] Update `./scripts/verify.sh` to run the structured test suite headlessly.
- [ ] TICKET-05: Headless Node & RID Leak Cleanup
    - [ ] Audit test teardown hooks to ensure all dynamically instantiated nodes call `QueueFree()` and drain the scene tree.
    - [ ] Ensure custom `RefCounted` or `Resource` handles and active tweens are cleanly terminated before test completion.
- [ ] TICKET-06: Deconstruct Pocket.cs
    - [ ] Extract arm physics and animation into a dedicated child node/helper (`PocketArmsController`).
    - [ ] Extract audio playback, pitch escalation, and stream selection into a dedicated audio component (`PocketAudioPlayer`).
    - [ ] Restrict `Pocket.cs` to ball acceptance, capacity checking, and payout event dispatching.
- [ ] TICKET-07: Streamline Launcher & Ball Dynamics
    - [ ] Replace async duplicate ball instantiation in `Launcher.cs` with direct, synchronous queue pops from Hopper.
    - [ ] Replace CDF distribution sampler with standard Godot `Curve.Sample(GD.Randf())` or explicit min/max ranges.
    - [ ] Retain wall-following vector projections and stuck detection in `Ball.cs`, but streamline transition state enums and signal dispatch.
- [ ] TICKET-08: Click-to-Apply Card Interaction
    - [ ] Remove `CardDragController.cs` and screen-bounding drag math.
    - [ ] Implement a discrete two-step selection flow: click card in hand/shop -> click target board socket/pocket.
    - [ ] Remove negative/downgrade pocket mutation archetypes that add clutter without tactical depth.
- [ ] TICKET-09: Simplify Card UI & Pip Indicators
    - [ ] Refactor `CardUI.cs` and `BallAwardIndicator.cs` into lightweight controls backed by standard Godot theme styles and layouts.
    - [ ] Use standard container layouts (`HBoxContainer`, `GridContainer`) for pip and ball indicators.

## 2026-08-29

- [x] wayfinder design pass


### Priority

### Backlog

## 2026-08-09

- [x] balls deflect smoothly off of wall surface (override velocity)
- [x] balls shoot in direction of launcher hole
- [x] launcher hole loads next ball as preview prior to firing
- [x] implement pocket arm behavior on events

## 2026-07-??

- [x] load hopper with random ball tiers at startup
    - [x] create a new BallsManager to track ball tiers
