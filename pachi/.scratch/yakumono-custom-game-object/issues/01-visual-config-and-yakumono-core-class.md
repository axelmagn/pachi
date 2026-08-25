# 01: VisualConfig Settings, GlobalEvents & Yakumono Core Visual Class

**What to build:** Centralized visual configuration settings in `VisualConfig` for Yakumono face graphics, frames, and fallbacks, signal definitions in `GlobalEvents`, and the core `Yakumono` C# class in `src/yakumono/` extending `Pocket` with multi-layer dual rendering bound to `VisualConfig` and live editor update capabilities.

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] `VisualConfig` includes `[ExportGroup("Yakumono")]` settings for face textures, jackpot face texture, base color, frame texture, and foreground texture.
- [ ] `GlobalEvents` includes `YakumonoStateChanged` and `YakumonoPaidOut` signal delegates and notification methods.
- [ ] `Yakumono` C# class (`[Tool]`, `[GlobalClass]`) created in `src/yakumono/Yakumono.cs` inheriting from `Pocket`.
- [ ] `Yakumono` supports multi-layer dual rendering (Frame, Face, Foreground) displaying texture assets when present and falling back to procedural shape tinting when textures are null.
- [ ] `Yakumono` subscribes to `VisualConfig.Changed` via `VisualConfigBinding`, updating rendering live in the Godot editor viewport.
- [ ] C# headless unit tests in `src/art/tests/VisualConfigTests.cs` verify property propagation, dual rendering fallbacks, and null handling.
