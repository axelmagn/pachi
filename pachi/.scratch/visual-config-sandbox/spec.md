# Feature Spec: VisualConfig & In-Editor Visual Showcase

Status: completed

## Problem Statement

As visual artists create and refine art assets and color palettes for Pachi, integrating these assets into the game currently requires modifying distributed node properties across various scene files (`.tscn`) or editing low-level C# scripts. Because the visual artist is non-technical, manipulating raw scene trees or code introduces high friction, risks accidental regressions to physics colliders or node wirings, and lacks an immediate, real-time feedback loop.

## Solution

A centralized visual customization system with editor-time reactivity:
1. A single, dedicated `VisualConfig` custom resource located at `res://src/art/visual_config.tres` that centralizes all global board colors, pin visuals, pocket visuals, and UI aesthetics in one place with clean inspector categorization and dual-render support (texture priority with procedural shape fallback).
2. Game elements (`Pin`, `Pocket`, `PocketBallsIndicator`, `BoundaryRect`, `CardUI`) implemented as `[Tool]` scripts that subscribe to `VisualConfig.Changed` in the editor, automatically updating their rendering live in the Godot 2D editor viewport whenever properties are adjusted in the Inspector dock.
3. A static `Visual Showcase` scene (`res://src/art/visual_showcase.tscn`) displaying side-by-side instances of all visual components so artists can view palette balance across the entire game in a single viewport.

## User Stories

1. As a visual artist, I want all game-wide visual settings (background color, boundary color, pin colors, pocket colors, card colors) centralized in a single configuration resource (`res://src/art/visual_config.tres`), so that I don't have to hunt across multiple scene files to change art parameters.
2. As a visual artist, I want to edit colors and drag-and-drop sprite textures directly in the Godot Inspector on `VisualConfig`, and immediately see the changes reflected across all open scenes in the 2D editor viewport without running the game.
3. As a visual artist, I want pins and pocket components to automatically render assigned sprite textures when present, with configurable scale and offset parameters on `VisualConfig`, so that I can calibrate visual alignment directly in the editor without editing image assets externally.
4. As a visual artist, I want pins and pocket components to fall back to procedural shapes when no sprite texture is assigned, so that the game remains fully functional and visual even with partial art coverage.
5. As a visual artist, I want a static `Visual Showcase` scene (`res://src/art/visual_showcase.tscn`) containing all game components side-by-side, so that I can evaluate global color harmony and asset scaling in a single view.
6. As a developer, I want `[Tool]` scripts to cleanly isolate editor rendering from runtime gameplay logic (singletons, audio players, physics signals), so that editor previewing never throws runtime null exceptions or corrupts scene state.
7. As a developer, I want the visual configuration to use clean Godot custom resources producing human-readable text diffs in version control.

## Implementation Decisions

- **Terminology & Vocabulary**:
  - The centralized visual settings resource is canonicalized as `VisualConfig` (`res://src/art/visual_config.tres`).
  - The static preview scene is canonicalized as `Visual Showcase` (`res://src/art/visual_showcase.tscn`).
- **Resource Architecture (`VisualConfig.cs`)**:
  - Located in `src/art/VisualConfig.cs` with `[GlobalClass]`.
  - Property setters invoke `EmitChanged()`.
  - Organized with `[ExportGroup]` categories:
    - *Environment*: Background color.
    - *Pins*: Pin base color, optional pin texture (`Texture2D`), texture scale (`float` uniform scale), texture offset (`Vector2`), hit flash color.
    - *Pockets*: Pocket indicator background color, indicator border color, arm procedural color, optional arm texture (`Texture2D`), arm texture scale (`float` uniform scale), arm texture offset (`Vector2`).
    - *Cards & UI*: Card panel background color, border color, indicator background color.
- **Tool Script & Subscription Lifecycle**:
  - Game elements (`Pin`, `Pocket`, `BoundaryRect`, `CardUI`, etc.) are marked `[Tool]`.
  - Nodes discover `VisualConfig` via `ResourceLoader.Load<VisualConfig>("res://src/art/visual_config.tres")` with an optional exported override field.
  - In `_EnterTree()`, nodes subscribe: `_config.Changed += OnVisualConfigChanged;`.
  - In `_ExitTree()`, nodes unsubscribe: `_config.Changed -= OnVisualConfigChanged;`.
  - An `ApplyVisualConfig(VisualConfig config)` method updates colors, textures, and triggers `QueueRedraw()`.
  - In `_Ready()`, if `Engine.IsEditorHint()` is true, gameplay setup (audio streams, singletons, signal connections) is skipped.
- **Dual-Rendering Strategy**:
  - Components hold both a procedural shape node (e.g. `CircleSprite`, `CapsuleSprite`) and a `Sprite2D` node.
  - When `config.Texture` is set, `Sprite2D` is shown and the procedural node is hidden. When null, the procedural node is shown with `config.Color` and `Sprite2D` is hidden.
- **Visual Showcase Scene**:
  - A static Godot scene (`res://src/art/visual_showcase.tscn`) containing background `ColorRect`, boundary rect, pin samples, pocket samples, and card samples.

## Testing Decisions

- Tests verify that `VisualConfig` properties correctly propagate to initialized nodes.
- Tests verify dual-render fallback logic (sprite texture priority vs procedural fallback).
- Tests verify that `[Tool]` nodes gracefully handle missing/null config files without throwing exceptions.

## Out of Scope

- Runtime interactive color picker overlays and runtime `ResourceSaver` buttons.
- Dynamic runtime animation/particle test triggers in the editor viewport.
- Ball sprite texturing (balls remain color-only as defined by their `BallVariant` tier resources).
