# 03: Pocket scene layout integration and showcase verification

**What to build:** Complete pocket indicator stacking and sizing in `pocket.tscn` with full opacity, validated visually in `visual_showcase.tscn` and programmatically across test suites.

**Blocked by:** 02: PocketBallsIndicator squircle pips and dynamic contrast outlines

**Status:** resolved

- [x] `InputsIndicator` node in `pocket.tscn` is set to `IsInputIndicator = true`, `Position = Vector2(0, 30)`, `Size = Vector2(26, 10)`, and `modulate = Color(1, 1, 1, 1)`.
- [x] `OutputsIndicator` node in `pocket.tscn` is set to `IsInputIndicator = false`, `Position = Vector2(0, 42)`, `Size = Vector2(26, 10)`, and `modulate = Color(1, 1, 1, 1)`.
- [x] `Pocket.cs` correctly binds and configures both indicator instances on ready/rebuild.
- [x] `visual_showcase.tscn` screenshot generation in `VisualConfigTests.cs` runs cleanly.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
