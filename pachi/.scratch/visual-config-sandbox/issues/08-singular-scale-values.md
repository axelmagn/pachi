# 08: Singular Scale Values in VisualConfig

**What to build:** Refactor sprite scale configuration properties in `VisualConfig` (`PinTextureScale` and `ArmTextureScale`) from `Vector2` to a single scalar `float` number. Both X and Y dimensions of target sprites will uniformly use this singular scale value.

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] `VisualConfig` updates `PinTextureScale` from `Vector2` to `float` (default `1.0f`).
- [x] `VisualConfig` updates `ArmTextureScale` from `Vector2` to `float` (default `1.0f`).
- [x] `Pin` applies `PinTextureScale` uniformly (`Vector2.One * PinTextureScale`) to `TextureSprite.Scale`.
- [x] `Pocket` applies `ArmTextureScale` uniformly (`Vector2.One * ArmTextureScale`) to arm sprites.
- [x] Existing resources (`res://src/art/visual_config.tres`) and scenes (`res://src/art/visual_showcase.tscn`) are updated to use scalar scale values.
- [x] Automated tests in `VisualConfigTests.cs` verify singular scale property propagation.
