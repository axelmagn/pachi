# 03: Scene Setup, Level Integration & Visual Showcase Preview

**What to build:** Complete Godot scene wiring for Yakumono: the `src/yakumono/yakumono.tscn` scene resource, a staging preview instance in `src/art/visual_showcase.tscn`, and replacement of the generic `CenterPocket` in `src/levels/level.tscn` with the new `Yakumono` object.

**Blocked by:** 02: Yakumono Face State Machine & Ball Interaction

**Status:** ready-for-agent

- [ ] `res://src/yakumono/yakumono.tscn` scene created with frame, face, and overlay visual nodes attached to `Yakumono` root.
- [ ] Staging instance added to [visual_showcase.tscn](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/src/art/visual_showcase.tscn) for live editor theme previews.
- [ ] Generic `CenterPocket` in [level.tscn](file:///C:/Users/Axel/workspace/axelmagn/pachi/pachi/src/levels/level.tscn) replaced with `Yakumono` instance.
- [ ] Headless build (`dotnet build Pachi.sln`) completes with 0 errors and 0 warnings.
- [ ] Headless editor (`godot-mono --headless --editor --quit`) and runtime (`godot-mono --headless --quit`) verify stability without errors.
