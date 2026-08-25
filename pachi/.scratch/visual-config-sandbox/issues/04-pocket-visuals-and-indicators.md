# 04: Pocket Components & Indicator Visual Reactivity

**What to build:** Pocket arm dual-rendering (sprite texture priority with procedural shape fallback) and pocket indicator visual styling driven by `VisualConfig` with live editor update capabilities.

**Blocked by:** 01: VisualConfig Resource & Settings Schema

**Status:** completed

- [x] `Pocket` and `PocketBallsIndicator` operate as `[Tool]` scripts subscribing to `VisualConfig.Changed`.
- [x] Pocket arms support dual rendering: displaying an arm texture when assigned in `VisualConfig`, or falling back to procedural shape rendering using `ArmColor`.
- [x] `PocketBallsIndicator` background color and border color dynamically update when `VisualConfig` is modified.
- [x] Audio streams, hole signals, and runtime gameplay triggers remain isolated and do not execute or throw errors in the editor (`Engine.IsEditorHint()`).
