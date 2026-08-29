# Pocket Indicator Legibility Technical Specification

**Status**: Ready for Implementation  
**ADR Reference**: [0002-pocket-indicator-legibility.md](../../docs/adr/0002-pocket-indicator-legibility.md)

## 1. Overview & Goals

Improve the legibility, visual scanability, and spatial distinction of pocket input and output indicators across the game and visual showcase scene.

### Key Requirements
- **Pip Pixel Maximization**: Render pips as full-height rounded squares (squircles) that utilize maximum available vertical height inside indicator rows.
- **Dynamic Contrast Outlines**: Compute pip stroke outlines dynamically from ball placeholder colors.
- **Distinct Input vs. Output Hues**: Add `InputIndicatorBackgroundColor` and `OutputIndicatorBackgroundColor` properties to `VisualConfig`.
- **Full Opacity Stacked Container**: Remove translucent `modulate` values and stack Input (Top Row) and Output (Bottom Row) indicators under pockets with crisp borders.

---

## 2. Component Design & Changes

### 2.1 `VisualConfig.cs`
- Deprecate/replace `IndicatorBackgroundColor` with:
  - `InputIndicatorBackgroundColor`: `Color("#1A2433")` (dark blue/slate hue)
  - `OutputIndicatorBackgroundColor`: `Color("#33221A")` (dark bronze/amber hue)
- Retain `IndicatorBorderColor` (`Color("#304A31")`) for tile borders.

### 2.2 `PocketBallsIndicator.cs`
- Add `[Export] public bool IsInputIndicator` flag (or align with `IsCardIndicator`).
- Update `_Draw()`:
  - Fill background rect using `InputIndicatorBackgroundColor` or `OutputIndicatorBackgroundColor`.
  - Draw full-height rounded-square pips (`DrawRect` with corner radius or `DrawStyleBox`).
  - Calculate pip stroke outline dynamically from `Balls[i].PlaceholderColor.Darkened(0.35f)`.
- Ensure opacity defaults to 1.0 (opaque).

### 2.3 `pocket.tscn` & `Pocket.cs`
- Update `InputsIndicator` position to `Vector2(0, 30)`, `Size` to `Vector2(26, 10)`, `modulate` to `Color(1, 1, 1, 1)`.
- Update `OutputsIndicator` position to `Vector2(0, 42)`, `Size` to `Vector2(26, 10)`, `modulate` to `Color(1, 1, 1, 1)`.

---

## 3. Verification & Acceptance Criteria

1. **C# Formatting & Build**: `.\scripts\verify.ps1` passes with 0 errors and 0 warnings.
2. **Headless Test Suite**: All unit & integration tests (`TestRunner.cs`) pass clean.
3. **Visual Showcase**: In-editor `visual_showcase.tscn` displays crisp, highly legible 2-row rounded-square pocket indicators.
