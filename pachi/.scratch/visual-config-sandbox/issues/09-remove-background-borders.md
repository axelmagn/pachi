# 09: Remove Background Borders

**What to build:** Remove the stroked border outline from `BoundaryRect` so that boundaries and environment backgrounds render cleanly without unrequested background bounding box borders.

**Blocked by:** None (can start immediately)

**Status:** completed

- [x] `BoundaryRect` removes custom border outline drawing (`DrawRect(..., filled: false)`).
- [x] Remove `BoundaryColor` from `BoundaryRect` and `VisualConfig` (or deprecate/clean up border references).
- [x] Ensure `BoundaryRect` and `EnvironmentBackground` continue rendering background fills cleanly without border strokes.
- [x] Update `res://src/art/visual_showcase.tscn` and `res://src/art/visual_config.tres` to remove boundary border settings.
- [x] Update tests in `VisualConfigTests.cs` to verify clean background rendering without boundary border requirements.
