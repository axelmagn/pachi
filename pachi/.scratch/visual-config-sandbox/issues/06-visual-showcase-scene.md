# 06: Visual Showcase Static Preview Scene

**What to build:** A dedicated static preview scene displaying side-by-side instances of all visual components (boundaries, pins, pockets, cards, UI) in the Godot 2D editor viewport, allowing artists to evaluate and refine global color balance in real-time.

**Blocked by:** 02: Environment & Boundary Live Visual Reactivity, 03: Pin Dual-Rendering & In-Editor Reactivity, 04: Pocket Components & Indicator Visual Reactivity, 05: Card UI Styling Reactivity

**Status:** completed

- [x] A static scene exists at `res://src/art/visual_showcase.tscn`.
- [x] Contains side-by-side instances of boundary rects, pins (both procedural and textured), pockets (with arms and indicators), and card UI elements against the configured background.
- [x] Modifying `res://src/art/visual_config.tres` in the Godot Inspector immediately updates all component instances within the showcase viewport simultaneously.
- [x] Opening and editing the showcase scene in Godot causes zero runtime/null exceptions in the editor console.
