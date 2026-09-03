# Pachi Architecture Audit & Refactoring Roadmap

Guidance for agents and contributors refactoring the Pachi codebase toward **architectural simplicity, test isolation, and tactile arcade deckbuilding**.

---

## 1. Guiding Principles

- **Simplicity Over Nuance**: Trade micro-optimizations and complex procedural hooks for clean Godot engine idioms (native scenes, exported node properties, standard theme resources).
- **Single Source of Truth**: Eliminate dual rendering pipelines, duplicate showcase scenes, and redundant design documents.
- **Test Isolation**: Separate zero-Godot pure domain logic from headless scene integration tests; ensure every test drains and disposes nodes without ObjectDB or RID leaks.
- **Click-to-Apply Ergonomics**: Prefer explicit, discrete click interactions over continuous drag-and-drop geometry math.

---

## 2. Refactoring Phases & Actionable Tickets

### Phase 1: Teardown & Dead Code Removal
*Eliminate obsolete abstraction layers, procedural shape draw hooks, and conflicting documentation.*

#### [x] TICKET-01: Remove VisualConfig Pipeline
- **Problem**: `VisualConfig`, `VisualConfigBinding`, and `visual_showcase.tscn` introduce a parallel procedural rendering pipeline, runtime `[Tool]` crashes, and test bloat.
- **Actions**:
  1. Delete `src/art/VisualConfig.cs`, `src/art/VisualConfigBinding.cs`, `src/art/visual_config.tres`, `src/art/visual_showcase.tscn`, and `src/art/tests/VisualConfigTests.cs`.
  2. Remove all `VisualConfigBinding` fields and `ApplyVisualConfig()` calls from `Pin.cs`, `Pocket.cs`, `CardUI.cs`, `BallAwardIndicator.cs`, `PocketBallsIndicator.cs`, `Yakumono.cs`, and `EnvironmentBackground.cs`.
  3. Replace visual properties with standard Godot scene `[Export]` properties (`Texture2D`, `Color`) and native `.tscn` configurations.
- **Completion Criteria**: Zero references to `VisualConfig` or `VisualConfigBinding` across the codebase; scenes render via native Godot nodes; `./scripts/verify.sh` passes.

#### [ ] TICKET-02: Remove Procedural Shape Sprites
- **Problem**: Custom `_Draw()` wrappers in `src/shape_sprites/` duplicate Godot's built-in 2D rendering capabilities.
- **Actions**:
  1. Delete `src/shape_sprites/` (all 6 sprite/collider classes).
  2. Replace scene references in `pocket.tscn`, `pin.tscn`, and other scenes with native Godot `Sprite2D`, `Polygon2D`, or `CollisionShape2D`.
- **Completion Criteria**: Directory `src/shape_sprites/` is removed; all scene collision and visual nodes instantiate native Godot nodes without errors.

#### [ ] TICKET-03: Prune Stale Documentation
- **Problem**: `docs/` contains conflicting and outdated specs that create context pollution.
- **Actions**:
  1. Retain `docs/notes/20260827_jacob_playtest.md`, `CONTEXT.md`, `CODE_STYLE.md`, and `AGENTS.md`.
  2. Archive or delete obsolete design specs in `docs/design/`, stale ADRs (e.g. `0001-visual-config-and-sandbox.md`), and unused scratch notes.
  3. Ensure `CONTEXT.md` serves as the sole domain reference for terminology and game mechanics.
- **Completion Criteria**: `docs/` contains only actively maintained reference documents, ADRs reflecting current architecture, and primary playtest notes.

---

### Phase 2: Testing Framework & Memory Leak Elimination
*Establish test isolation, structured runners, and headless leak cleanup.*

#### [x] TICKET-04: Structured Godot C# Test Framework
- **Problem**: `TestRunner.cs` relies on static reflection with fail-fast halts, lacks test isolation, and conflates domain logic with scene rendering.
- **Actions**:
  1. Adopt Chickensoft `GoDotTest` and `Shouldly`.
  2. Establish discrete test classes using `[TestClass]`, `[Test]`, and `[Cleanup]` lifecycles.
  3. Split test suites into pure domain unit tests (`src/<domain>/tests/`) and scene integration tests (`tests/integration/`).
  4. Remove legacy screenshot capture assertions and runners.
  5. Update `./scripts/verify.sh` and `./scripts/verify.ps1` to run the structured test suite headlessly.
- **Completion Criteria**: Individual test failures do not block subsequent tests; test results output clear pass/fail/skip summaries; `./scripts/verify.sh` executes the suite and exits `0`.

#### [x] TICKET-05: Headless Node & RID Leak Cleanup
- **Problem**: Headless test runs leak `CanvasItem` RIDs and `ObjectDB` instances on exit.
- **Actions**:
  1. Audit test teardown hooks to ensure all dynamically instantiated nodes call `QueueFree()` and drain the scene tree.
  2. Ensure custom `RefCounted` or `Resource` handles and active tweens are cleanly terminated before test completion.
- **Completion Criteria**: Headless execution of `./scripts/verify.sh` completes with zero RID or ObjectDB leak warnings.

---

### Phase 3: Core Entity Deconstruction & Physics Simplification
*Decompose oversized monolithic classes into single-responsibility nodes.*

#### [x] TICKET-06: Deconstruct Pocket.cs
- **Problem**: `Pocket.cs` acts as a god object managing tulip arm tweening, audio pitch math, visual indicator layouts, drag targets, and ball physics reception.
- **Actions**:
  1. Extract arm physics and animation into a dedicated child node/helper (`PocketArmsController`).
  2. Extract audio playback, pitch escalation, and stream selection into a dedicated audio component (`PocketAudioPlayer`).
  3. Restrict `Pocket.cs` to ball acceptance, capacity checking, and payout event dispatching.
- **Completion Criteria**: `Pocket.cs` delegates arm animation and sound playback to child components; each component is independently testable.

#### [ ] TICKET-07: Streamline Launcher & Ball Dynamics
- **Problem**: `Launcher.cs` uses async node cloning and complex CDF binary search sampling for power variations.
- **Actions**:
  1. Replace async duplicate ball instantiation in `Launcher.cs` with direct, synchronous queue pops from Hopper.
  2. Replace CDF distribution sampler with standard Godot `Curve.Sample(GD.Randf())` or explicit min/max ranges.
  3. Retain wall-following vector projections and stuck detection in `Ball.cs`, but streamline transition state enums and signal dispatch.
- **Completion Criteria**: Launcher operates synchronously without orphan node clones; ball launching exhibits consistent, tunable power distributions.

---

### Phase 4: Card System & UI Rearchitecture
*Replace complex drag-and-drop bounding-box math with clean click-to-apply interactions.*

#### [ ] TICKET-08: Click-to-Apply Card Interaction
- **Problem**: `CardDragController.cs` requires complex viewport-to-screen coordinate math and continuous spatial proximity checks.
- **Actions**:
  1. Remove `CardDragController.cs` and screen-bounding drag math.
  2. Implement a discrete two-step selection flow: click card in hand/shop -> click target board socket/pocket.
  3. Remove negative/downgrade pocket mutation archetypes that add clutter without tactical depth.
- **Completion Criteria**: Card application operates via explicit click selection; `CardDragController.cs` is deleted; UI state transitions cleanly between idle, targeting, and applied.

#### [ ] TICKET-09: Simplify Card UI & Pip Indicators
- **Problem**: UI components mix tool-mode visual updates, custom redraw hooks, and dynamic style overrides.
- **Actions**:
  1. Refactor `CardUI.cs` and `BallAwardIndicator.cs` into lightweight controls backed by standard Godot theme styles and layouts.
  2. Use standard container layouts (`HBoxContainer`, `GridContainer`) for pip and ball indicators.
- **Completion Criteria**: UI scenes render cleanly in the Godot editor without requiring runtime tool script evaluation or custom draw loops.
