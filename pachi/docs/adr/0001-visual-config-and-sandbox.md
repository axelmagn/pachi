# 1. Centralized VisualConfig and In-Editor Tool Visual Showcase

We manage all game-wide visual settings (palettes, sprite assignments, and visual parameters) via a single flat `VisualConfig` custom resource (`res://src/art/visual_config.tres`), paired with `[Tool]` game elements that subscribe to its `Changed` signal in the editor, and a static `visual_showcase.tscn` staging scene.

## Context & Trade-offs
We needed a centralized, foolproof way for a visual artist to tune color palettes and assign 2D sprite textures without editing fragile scene hierarchies or colliding with Godot's built-in UI `Theme` system.
- We rejected an interactive in-game runtime sandbox scene because building custom runtime color pickers and `ResourceSaver` persistence UI introduced unnecessary maintenance overhead.
- We rejected a custom C# `EditorPlugin` main-screen tab due to high development friction and version-migration complexity across Godot updates.
- We chose editor-time `[Tool]` reactivity: game elements (`Pin`, `Pocket`, `BoundaryRect`, `CardUI`) load `res://src/art/visual_config.tres`, hook `VisualConfig.Changed`, and update their rendering live in the Godot 2D viewport. A static `visual_showcase.tscn` scene provides an all-in-one staging view.
