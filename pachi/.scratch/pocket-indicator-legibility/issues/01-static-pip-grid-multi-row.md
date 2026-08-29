# 01: Static 8x8 Pip Grid and Multi-Row Indicator Rendering

**What to build:** Multi-row squircle pip rendering in `PocketBallsIndicator` utilizing static 8x8 px pips with 0px spacing (border-to-border), 1px outer indicator border (`Size.X = 34px`, `Size.Y = 10px` for 1 row, `18px` for 2 rows), centered rows for < 4 balls, and dynamic contrast stroke outlines.

**Blocked by:** None (can start immediately)

**Status:** closed

- [x] `PocketBallsIndicator` renders static 8x8 px squircle pips with 2px corner radius.
- [x] Horizontal and vertical gap between adjacent pips is 0 px (border-to-border).
- [x] Indicator frame automatically sizes to 34x10 px for 1 row (1–4 balls) and 34x18 px for 2 rows (5–8 balls).
- [x] Rows with fewer than 4 balls center their pips horizontally within the indicator frame.
- [x] Pip counts are clamped to a maximum of 8 balls across at most 2 rows of 4 columns.
- [x] Unit tests in `VisualConfigTests.cs` validate pip grid dimensions, row wrapping, and center alignments.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
