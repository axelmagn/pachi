# Yakumono Custom Game Object Specification

**Status:** ready-for-agent  
**Feature Slug:** `yakumono-custom-game-object`

---

## Problem Statement

The centerpiece of pachinko machines is the **Yakumono**—a specialized mechanical feature with custom visual flair, dynamic behaviors, and enhanced payout mechanics. Currently, the central feature in Pachi is represented by a generic `Pocket` instance with `IsCentralPocket = true` and `HasArms = false`. This lacks a distinct visual identity (such as animated face states), customizable multi-layer graphics, and state-machine transitions needed to make the centerpiece feel like a full-fledged, customizable game object.

## Solution

Specialized `Yakumono` as a custom C# class inheriting from `Pocket` (`public partial class Yakumono : Pocket`), placed in `src/yakumono/` with a dedicated scene `yakumono.tscn`. The `Yakumono` features an internal state machine driving character face transitions (such as a moon character making different faces). Upon catching a ball, it randomly cycles to a new face state from an assigned sprite array. On payout, it transitions to a reserved Jackpot face state. Visual assets (face texture arrays, multi-layer frames, procedural colors) are centralized in `VisualConfig` (`res://src/art/visual_config.tres`), enabling live updating in the Godot editor and `visual_showcase.tscn`. Rich global signals (`YakumonoStateChanged`, `YakumonoPaidOut`) are added to `GlobalEvents` to trigger gamefeel and audio feedback.

## User Stories

1. As a player, I want the centerpiece of the board to feature a distinct visual character (such as a cute moon with changing faces), so that the board feels alive and engaging.
2. As a player, I want the centerpiece face to react and change every time a ball enters the pocket, so that I get immediate visual feedback for my shots.
3. As a player, I want a special Jackpot face to display when the centerpiece pays out its reward, so that payouts feel celebratory and distinct from normal hits.
4. As a game artist, I want to configure an array of moon face textures in `VisualConfig`, so that I can easily add, remove, or swap character faces without modifying C# code.
5. As a game artist, I want `Yakumono` graphics to update live in the Godot 2D editor viewport when editing `VisualConfig`, so that I can rapidly iterate on visuals in `visual_showcase.tscn`.
6. As a game artist, I want procedural color fallbacks when face textures are missing, so that I can design and place Yakumono objects before final art assets are finalized.
7. As a game developer, I want `Yakumono` to inherit from `Pocket`, so that existing ball capture, slot management, ball variant outputs, and hole physics logic are reused cleanly.
8. As a game developer, I want `GlobalEvents` to emit `YakumonoStateChanged` and `YakumonoPaidOut` signals, so that camera shake, sound effects, and card mechanics can react to centerpiece transitions.
9. As a level designer, I want a pre-built `yakumono.tscn` scene in `src/yakumono/`, so that I can place centerpiece features into level scenes.

## Implementation Decisions

- **Class Inheritance & Domain Boundaries**:
  - `Yakumono` is implemented as a partial C# class `public partial class Yakumono : Pocket` residing in domain directory `src/yakumono/Yakumono.cs`.
  - Decorated with `[Tool]` and `[GlobalClass]`.
- **State Machine Mechanics**:
  - Encapsulates an integer state index representing the current face state.
  - On ball entry (`OnBallCatch`), picks a random index from `FaceTextures` (excluding the current face to guarantee a visual change).
  - On payout completion (`NotifyCentralPocketPaidOut`), switches to a reserved Jackpot face state.
- **`VisualConfig` Integration**:
  - `VisualConfig.cs` expanded with `[ExportGroup("Yakumono")]` containing `Array<Texture2D> FaceTextures`, `Texture2D JackpotFaceTexture`, `Color YakumonoBaseColor`, `Texture2D FrameTexture`, and `Texture2D ForegroundTexture`.
  - Uses `VisualConfigBinding` to subscribe to `VisualConfig.Changed` and apply live visual updates in editor hints and runtime without executing gameplay scripts during editor hint processing.
- **Global Events Integration**:
  - `GlobalEvents.cs` extended with `YakumonoStateChanged(Yakumono yakumono, int previousState, int newState)` and `YakumonoPaidOut(Yakumono yakumono)` delegates and notification methods.
- **Scene Integration**:
  - `src/yakumono/yakumono.tscn` created as a Godot scene.
  - `src/art/visual_showcase.tscn` updated with a staging `Yakumono` instance.
  - `src/levels/level.tscn` updated to replace generic `CenterPocket` with `Yakumono`.

## Testing Decisions

- **Good Test Criteria**: Tests must verify external behaviors (property propagation, dual rendering priority, state transitions on ball catch and payout, signal emissions, null safety) rather than private implementation details.
- **Testing Seam**:
  - Primary Seam: C# Headless Test Suite (`VisualConfigTests.cs` called via `TestRunner.cs`).
  - Secondary Seam: Godot Engine Headless Verification (`godot-mono --headless --editor --quit` and `godot-mono --headless --quit`).
- **Prior Art**: Extends existing pattern in `src/art/tests/VisualConfigTests.cs`.

## Out of Scope

- Specific state-machine behaviors per face state beyond face graphic cycling (e.g., custom movement, physical rotating motors, dynamic pin shifts per face).
- Final finalized art assets (placeholder textures and procedural fallbacks used until artist provides moon face textures).
- Runtime theme persistence UI or custom in-game color picker UI.

## Further Notes

- Uses domain terms from [CONTEXT.md](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/CONTEXT.md) (`Yakumono`, `VisualConfig`, `Visual Showcase`, `Ball Variant`, `Shape Sprite`).
- Follows [0001-visual-config-and-sandbox.md](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/docs/adr/0001-visual-config-and-sandbox.md) editor-time `[Tool]` reactivity guidelines.
