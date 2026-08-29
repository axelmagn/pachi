# 02: PocketBallsIndicator squircle pips and dynamic contrast outlines

**What to build:** Squircle pip rendering in `PocketBallsIndicator` utilizing maximum row height with contrast stroke outlines dynamically darkened from ball placeholder colors, driven by an input/output indicator toggle.

**Blocked by:** 01: VisualConfig distinct input/output indicator colors

**Status:** resolved

- [x] `PocketBallsIndicator` exposes an exported `IsInputIndicator` property.
- [x] Background rect color resolves to `InputIndicatorBackgroundColor` or `OutputIndicatorBackgroundColor` depending on `IsInputIndicator` (or `CardIndicatorBackgroundColor` when `IsCardIndicator` is enabled).
- [x] `_Draw()` renders full-height rounded-square (squircle) pips inside the indicator bounds.
- [x] Pip stroke outlines are dynamically computed from `PlaceholderColor.Darkened(0.35f)`.
- [x] Unit tests in `VisualConfigTests.cs` cover indicator propagation and squircle options.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
