# Pachi Domain Context

Pachi is a physics-based pachinko / roguelike deckbuilder game developed in Godot 4.7 with C#.

## Language

**VisualConfig**:
A centralized data resource located at `res://src/art/visual_config.tres` containing the game's visual customization settings, including color palettes, sprite textures, and environmental aesthetics.
_Avoid_: Theme (conflicts with Godot's UI `Theme` system), Skin

**Visual Showcase**:
A static in-editor scene (`res://src/art/visual_showcase.tscn`) displaying side-by-side instances of all visual game components (pins, pockets, boundaries, cards, UI) used to preview live `VisualConfig` property edits in the Godot 2D editor viewport.
_Avoid_: Design Sandbox, Theme Editor, Test Level, Dev Playground

**Ball Variant**:
A configuration resource defining the gameplay attributes (tier price) and color representation for a specific ball tier.
_Avoid_: Ball Type, Ball Model

**Shape Sprite**:
A procedural vector drawing component used as a placeholder before final sprite textures are assigned.
_Avoid_: Dummy Sprite, Placeholder Graphic

**Stuck Ball**:
An active ball in play whose displacement remains within a minimum spatial threshold for longer than the detection window.
_Avoid_: Jammed Ball, Dead Ball

**Nudge**:
An automatic upward and lateral impulse applied to a stuck ball to dislodge it from pins, pockets, or bottlenecks.
_Avoid_: Shake, Tilt, Jolt

**Ball Refund**:
The recovery process that despawns a persistently stuck ball after failed nudges and re-queues an equivalent ball variant in the hopper.
_Avoid_: Despawn Penalty, Void Ball, Ball Recycle

