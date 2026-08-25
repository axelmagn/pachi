# 05: Card UI Styling Reactivity

**What to build:** In-editor and runtime visual styling for Card UI components driven by `VisualConfig` settings.

**Blocked by:** 01: VisualConfig Resource & Settings Schema

**Status:** completed

- [x] `CardUI` operates as a `[Tool]` script and subscribes to `VisualConfig.Changed`.
- [x] Card background panel, border styling, and indicator background colors update live when `VisualConfig` is edited.
- [x] Card drag-and-drop controller and runtime gameplay interactions are guarded against execution in editor mode.
