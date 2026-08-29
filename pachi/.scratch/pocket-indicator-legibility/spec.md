# Pocket Indicator Multi-Row Static Pip Grid Specification

**Status**: `implemented`  
**ADR Reference**: [0002-pocket-indicator-legibility.md](../../docs/adr/0002-pocket-indicator-legibility.md)

## Problem Statement

In the main game and visual showcase, pocket indicator pips scale down dynamically as more balls are added. This scaling makes the pips look inconsistent and ugly. Scaled-down pips also fail to fill the available indicator row height, degrading scanability and legibility. Furthermore, pockets lack strict constraints on input and output ball counts, leading to visual clutter and layout overlap.

## Solution

Standardize indicator pips to a static 8x8 px squircle size with zero padding between adjacent pips (relying on dynamic contrast borders). Pockets enforce a hard cap of maximum 4 input balls (single row of up to 4 pips) and maximum 8 output balls (up to 2 rows of 4 pips each). Output indicators dynamically expand vertically and adjust their position when two rows are present.

## User Stories

1. As a player, I want pocket indicator pips to remain a constant static size regardless of how many balls are queued, so that the UI looks consistent and balanced.
2. As a player, I want pips to fill the full vertical height of their indicator row, so that ball colors are crisp and identifiable at a glance.
3. As a player, I want pips to sit edge-to-edge with no interior gap, so that the indicator tile remains compact without wasted space.
4. As a player, I want pocket input requirements to never exceed 4 balls, so that goals remain clear and fit cleanly on a single row.
5. As a player, I want pocket payout outputs to support up to 8 balls across 2 rows of 4, so that high-reward pockets display clearly without shrinking pips.
6. As a player, I want output indicators with 5 to 8 balls to wrap cleanly into a second row, so that all reward balls remain legible.
7. As a player, I want the output indicator to adjust its vertical position when the row count changes, so that indicators never overlap.
8. As a deckbuilder player, I want cards that add input balls to be disabled when a pocket already has 4 inputs, so that I cannot exceed the pocket capacity.
9. As a deckbuilder player, I want cards that add output balls to be disabled when a pocket already has 8 outputs, so that I cannot exceed the pocket capacity.
10. As a player, I want CardUI ball indicators to share the same static 8x8 squircle pip rendering, so that visual language is unified across the UI.

## Implementation Decisions

1. **Static Pip Geometry & Layout**:
   - Pips are drawn as static 8x8 px rounded squares (squircles) with a 2px corner radius.
   - Horizontal and vertical spacing between pips within an indicator is 0 px (border-to-border).
   - Indicator outer border is 1 px (`Size.X = 34px` for 4 columns; `Size.Y = 10px` for 1 row, `18px` for 2 rows).
   - Rows with fewer than 4 pips center their pips horizontally within the indicator frame.

2. **Pocket Capacity Guardrails**:
   - Pocket inputs hard limit: maximum 4 balls.
   - Pocket outputs hard limit: maximum 8 balls.
   - Add-input card mechanics and archetypes reject targets with 4 or more input balls.
   - Add-output card mechanics and archetypes reject targets with 8 or more output balls.

3. **Dynamic Multi-Row Indicator & Stacking**:
   - `PocketBallsIndicator` renders up to 2 rows of 4 pips each (clamped at 8 pips max).
   - `InputsIndicator` always occupies 1 row (`34x10 px`).
   - `OutputsIndicator` renders 1 row (`34x10 px`) for 1–4 balls, and 2 rows (`34x18 px`) for 5–8 balls.
   - In `Pocket` and `Yakumono`, `OutputsIndicator.Position.Y` dynamically positions below `InputsIndicator` accounting for indicator heights and vertical padding.

4. **Visual Showcase & Cards Integration**:
   - `CardUI` and card helper routines format multi-ball packs and transitions using the static pip dimensions.
   - Showcase scene reflects the 34px width indicator layout.

## Testing Decisions

- Test external behavior through existing test harnesses:
  - Unit tests in `VisualConfigTests.cs` validating indicator row wrapping, dimensions, and static pip bounds.
  - Card application tests asserting that `CanApply` returns `false` when input count == 4 or output count == 8.
  - Verification suite (`verify.ps1` / `verify.sh`) ensuring zero warnings, analyzer compliance, and passing test runner suite.

## Out of Scope

- Dynamic font scaling or text badges inside pocket indicators.
- 3+ row indicators or horizontal scrolling indicators.
- Changing pin positions or pocket physics colliders.

## Further Notes

- Maintains existing dynamic stroke outline calculation (`Darkened(0.35f)` / `Lightened(0.25f)`) and distinct input (`#1A2433`) vs output (`#33221A`) background hues established in ADR 0002.

