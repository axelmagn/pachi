# 03: Pin Dual-Rendering & In-Editor Reactivity

**What to build:** Dual-rendering support for pins (displaying an assigned sprite texture when present, or falling back to a procedural shape sprite when no texture is assigned) and live editor viewport reactivity when `VisualConfig` is adjusted.

**Blocked by:** 01: VisualConfig Resource & Settings Schema

**Status:** completed

- [x] `Pin` operates as a `[Tool]` script, subscribing to `VisualConfig.Changed` in `_EnterTree()` and unsubscribing in `_ExitTree()`.
- [x] When `PinTexture` is set on `VisualConfig`, `Pin` displays the sprite texture; when null, it falls back to procedural drawing using `PinBaseColor`.
- [x] Hit flash color and base colors update live in the 2D editor viewport upon inspector edits to `VisualConfig`.
- [x] Gameplay mechanics (hit particles, audio, physics signals) are safely skipped or isolated when running inside the editor (`Engine.IsEditorHint()`).
