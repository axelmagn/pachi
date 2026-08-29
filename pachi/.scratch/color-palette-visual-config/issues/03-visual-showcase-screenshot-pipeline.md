# 03: Synchronize visual showcase and add headless screenshot verification pipeline

**What to build:** `visual_config.tres` and `visual_showcase.tscn` are synchronized with updated default colors, and an automated screenshot test method in `VisualConfigTests.cs` / `TestRunner.cs` exports `.scratch/visual_showcase.png` in headless mode for visual verification.

**Blocked by:** #01: Update VisualConfig default color palette and property tests, #02: Align Ball Variant tier resources with Ball Palette

**Status:** resolved

- [x] `src/art/visual_config.tres` and inline resources in `src/art/visual_showcase.tscn` reflect updated default palette colors.
- [x] `VisualConfigTests.cs` contains a screenshot capture test method that instantiates `visual_showcase.tscn`, processes render frames, captures the viewport image, and saves `.scratch/visual_showcase.png`.
- [x] `TestRunner.cs` invokes the screenshot capture test during headless execution (`.\scripts\verify.ps1` or `./scripts/verify.sh`).
- [x] `.scratch/visual_showcase.png` is generated cleanly as a non-empty image file.
- [x] Verification script (`.\scripts\verify.ps1` or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
