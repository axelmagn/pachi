Status: resolved

# Spec: Color Palette Implementation in VisualConfig

## Problem Statement

The game's visual configuration resource (`VisualConfig`) and default resource (`visual_config.tres`) currently use hardcoded arbitrary placeholder colors rather than the unified color palette defined in `docs/palette.md` (extracted from `assets/sprites/ellis/palette01.JPG`). Additionally, property names in `VisualConfig` must remain decoupled from specific color palette names (such as `DarkestGreen` or `SlateBlue`) to ensure that future palette changes do not break API contracts or property names. Finally, ball tier resources need to align with the Ball Palette, and an automated headless visual verification helper is required.

## Solution

1. Update `VisualConfig.cs` to expose semantic game element color properties organized into logical export groups (`Environment`, `Pins`, `Pockets`, `Cards & UI`, `Yakumono`, and `Ball Tiers`).
2. Assign the hex colors from `docs/palette.md` as default values for these semantic element properties, keeping property names strictly decoupled from specific color palette names.
3. Update `PlaceholderColor` in ball variant tier resources (`tier_1.tres` through `tier_6.tres` and `default_ball_variant.tres`) to match the semantic ball tier colors from the Ball Palette.
4. Synchronize `src/art/visual_config.tres` and `src/art/visual_showcase.tscn` to reflect the updated default values.
5. Provide an automated screenshot generation helper in `TestRunner.cs` / `VisualConfigTests.cs` that renders `visual_showcase.tscn` in headless Godot to export `.scratch/visual_showcase.png`, enabling visual verification by the implementer agent.

## User Stories

1. As a game designer, I want all color properties in `VisualConfig` to use semantic game element names (e.g., `BackgroundColor`, `PinBaseColor`, `CardBackgroundColor`), so that property names remain stable when the art palette changes.
2. As a game designer, I want default color values in `VisualConfig` to be initialized from the palette defined in `docs/palette.md`, so that the default game theme matches the target art direction.
3. As a game designer, I want to edit any element color property in the Godot Inspector or override resource files (`visual_config.tres`), so that visual themes can be customized per level or preset.
4. As a player, I want the environment background to render in dark green (`#1C261D`), so that the game board presents a cohesive dark aesthetic.
5. As a player, I want pins on the board to render in slate blue (`#B9CBD9`) by default and flash cream yellow (`#F6E8A9`) on collision, so that pin interactions are visually crisp.
6. As a player, I want pocket arms to render in sage green (`#7B924E`), pocket indicator backgrounds to render in dark green (`#243026`), and indicator borders to render in forest green (`#304A31`), so that pockets stand out clearly against the background.
7. As a player, I want card UI elements to render with espresso brown backgrounds (`#452A21`) and terracotta amber borders (`#D2814A`), so that card choices look cohesive.
8. As a player, I want the Yakumono feature to render in vermilion coral (`#CC6542`), so that the centerpiece feature remains visually prominent.
9. As a player, I want ball variants to render in the dedicated ball color palette based on tier (`tier_1` `#F3E8AA`, `tier_2` `#EAB879`, `tier_3` `#D1814C`, `tier_4` `#CA6642`, `tier_5`/`tier_6` `#C04D38`), so that ball tiers are visually distinguishable.
10. As a developer or agent, I want the automated verification suite to render `visual_showcase.tscn` and export `.scratch/visual_showcase.png`, so that I can visually verify the rendering of the color palette.
11. As a developer, I want all unit tests in `VisualConfigTests.cs` to pass with zero errors and zero warnings, so that visual config changes do not introduce regressions.

## Implementation Decisions

- **Semantic Element Properties**: Preserve and extend semantic property names in `VisualConfig.cs` without introducing color-name properties:
  - `[ExportGroup("Environment")]`: `BackgroundColor` (default `#1C261D`)
  - `[ExportGroup("Pins")]`: `PinBaseColor` (default `#B9CBD9`), `FlashColor` (default `#F6E8A9`)
  - `[ExportGroup("Pockets")]`: `IndicatorBackgroundColor` (default `#243026`), `IndicatorBorderColor` (default `#304A31`), `ArmColor` (default `#7B924E`)
  - `[ExportGroup("Cards & UI")]`: `CardBackgroundColor` (default `#452A21`), `CardBorderColor` (default `#D2814A`), `CardIndicatorBackgroundColor` (default `#1C261D`)
  - `[ExportGroup("Yakumono")]`: `YakumonoBaseColor` (default `#CC6542`)
  - `[ExportGroup("Ball Tiers")]`: `BallTier1Color` (`#F3E8AA`), `BallTier2Color` (`#EAB879`), `BallTier3Color` (`#D1814C`), `BallTier4Color` (`#CA6642`), `BallTier5Color` (`#C04D38`)
- **Decoupled Architecture**: Do not expose palette color names (`DarkestGreen`, `SlateBlue`, etc.) as public properties or API symbols in `VisualConfig.cs`. Keep all property names focused strictly on game element semantics so that swapping palettes in the future requires updating default values only.
- **Ball Variants Configuration**: Update ball tier `.tres` files (`src/balls/tiers/tier_1.tres` through `tier_6.tres` and `default_ball_variant.tres`) to set `PlaceholderColor` to their corresponding Ball Palette colors.
- **Resource and Showcase Synchronization**: Update `src/art/visual_config.tres` and inline resources in `src/art/visual_showcase.tscn` to reflect the updated default values.
- **Headless Screenshot Capture**: Add a screenshot verification test method in `VisualConfigTests.cs` called during headless test execution (`TestRunner.cs`) that instantiates `visual_showcase.tscn`, processes 2 render frames, captures the viewport texture image, saves `.scratch/visual_showcase.png`, and verifies non-empty file creation.

## Testing Decisions

- **Good Test Criteria**: Test external behavior and visual contract propagation using semantic game element names. Verify default property values match the intended palette, property change notifications (`Changed`) fire on mutation, colors propagate to nodes (`BoundaryRect`, `Pin`, `Pocket`, `CardUI`, `Yakumono`), and screenshot artifacts are generated cleanly.
- **Modules Tested**: `VisualConfig`, `VisualConfigBinding`, `BoundaryRect`, `Pin`, `Pocket`, `PocketBallsIndicator`, `CardUI`, `Yakumono`, `TestRunner`.
- **Prior Art**: `VisualConfigTests.cs` (existing unit tests for VisualConfig property propagation and dual rendering fallbacks) and `TestRunner.cs` (headless test execution pipeline).

## Out of Scope

- Creating new custom sprite textures for pins, pockets, or yakumono (focus is purely on color palette implementation and shape sprite / modulation styling).
- Modifying physics behavior, ball movement, or card gameplay mechanics.
- Adding runtime color palette switching UI menus for players in main game loop.

## Further Notes

- The implementer agent must run `.\scripts\verify.ps1` (or `./scripts/verify.sh`) to execute the C# format check, build verification with zero warnings, test runner execution, and visual screenshot generation.
- The implementer agent must inspect `.scratch/visual_showcase.png` using `view_file` to visually confirm color accuracy before completing the task.
