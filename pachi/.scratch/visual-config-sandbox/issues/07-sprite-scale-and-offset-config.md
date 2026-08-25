# 07: Sprite Scale & Offset Configuration

**What to build:** Add scale and offset transformation variables to `VisualConfig` (`PinTextureScale`, `PinTextureOffset`, `ArmTextureScale`, `ArmTextureOffset`), allowing visual artists to fine-tune sprite alignment, pivot offsets, and asset scaling directly within the Inspector dock in real-time.

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] `VisualConfig` includes exported transformation properties:
  - `Pins`: `PinTextureScale` (`Vector2`, default `Vector2.One`), `PinTextureOffset` (`Vector2`, default `Vector2.Zero`).
  - `Pockets`: `ArmTextureScale` (`Vector2`, default `Vector2.One`), `ArmTextureOffset` (`Vector2`, default `Vector2.Zero`).
- [x] Modifying scale or offset in `VisualConfig` triggers `EmitChanged()`.
- [x] `Pin` applies `PinTextureScale` and `PinTextureOffset` to its `TextureSprite` node during `ApplyVisualConfig(...)`.
- [x] `Pocket` applies `ArmTextureScale` and `ArmTextureOffset` (with appropriate horizontal mirroring for left/right arms) to its arm sprite nodes during `ApplyVisualConfig(...)`.
- [x] In-editor adjustments in `VisualConfig` update pin and pocket sprite scaling and position live in the 2D editor viewport.
- [x] Automated tests in `VisualConfigTests.cs` verify that scale and offset properties correctly propagate and apply to `Pin` and `Pocket` sprite nodes.
