# 2. Pocket Indicator Legibility and Pip Rendering

We improve the scanability and legibility of pocket input and output indicators by rendering pips as full-row rounded squares with dynamically calculated contrast outlines, and visually separating input vs. output indicators using distinct background tile hues in a stacked 2-row container.

## Context & Trade-offs

The previous pocket indicator implementation rendered small circular dots (`DotRadius = 1.5f`) inside semi-translucent (`modulate.A ≈ 0.59`) indicator rectangles under the pocket. This presented three legibility defects:
1. **Low Pixel Allocation for Pips**: Small circular dots occupied very few pixels, making ball tier colors hard to distinguish at standard viewing distances or on mobile displays.
2. **Poor Contrast**: Indicator tiles used translucent modulations that bled into the dark green playfield background (`#1C261D`).
3. **Indistinguishable Flow Direction**: Input indicators (accepted balls) and Output indicators (spawned/paid out balls) shared identical styling, relying solely on subtle vertical offsets.

We evaluated three architectural alternatives:
- **Miniature Texture Sprites**: Rendering actual ball textures for each pip. Rejected due to high visual noise and poor legibility when scaled down to small indicator tiles.
- **Numerical Badges (e.g. x3)**: Grouping identical ball tiers with numeric text overlays. Rejected because multi-ball pockets are common and 1-to-1 visual pip alignment provides faster non-verbal spatial scanning.
- **Full-Row Rounded Square Pips with Distinct Tile Hues**: Rendering pips as full-height rounded squares (squircles) that maximize pixel coverage, calculating pip outlines dynamically from pip color, and styling Input vs. Output background tiles with distinct hues configured via `VisualConfig`. Selected.

## Decision

1. **Full-Row Rounded Square Pips**: Update `PocketBallsIndicator._Draw()` to render pips as rounded rectangles (`DrawSquare` / `DrawRect` with corner radius) occupying the full interior height of the indicator row.
2. **Dynamic Pip Outlines**: Calculate pip border outlines dynamically from each ball's `PlaceholderColor` (e.g., `Darkened(0.4f)` for light colors or `Lightened(0.2f)` for dark colors) to guarantee distinct edge separation without extra `VisualConfig` boilerplate.
3. **Distinct Background Tile Hues**: Add `InputIndicatorBackgroundColor` (default `#1A2433`) and `OutputIndicatorBackgroundColor` (default `#33221A`) to `VisualConfig`.
4. **Stacked 2-Row Container Layout**: Position input indicators (Top Row) and output indicators (Bottom Row) with full 100% opacity (`modulate.A = 1.0`) under `Pocket` nodes.
