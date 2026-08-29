# 02: Pocket and Yakumono Dynamic Stacking and Capacity Clamps

**What to build:** Hard capacity clamping (max 4 input balls, max 8 output balls) in `Pocket` and `Yakumono`, dynamic repositioning of `OutputsIndicator` vertically below `InputsIndicator` depending on indicator row count and height, and updated layout defaults in `pocket.tscn` and `yakumono.tscn`.

**Blocked by:** 01: Static 8x8 Pip Grid and Multi-Row Indicator Rendering

**Status:** closed

- [x] `Pocket` and `Yakumono` clamp input balls to a maximum of 4 and output balls to a maximum of 8.
- [x] `Pocket` and `Yakumono` dynamically position `OutputsIndicator.Position.Y` below `InputsIndicator` based on whether `OutputsIndicator` is 1 row or 2 rows.
- [x] `pocket.tscn` and `yakumono.tscn` default indicator nodes have `Size.X = 34` and appropriate initial positions.
- [x] Unit and scene integration tests verify indicator positioning and ball clamping on build and mutation.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
