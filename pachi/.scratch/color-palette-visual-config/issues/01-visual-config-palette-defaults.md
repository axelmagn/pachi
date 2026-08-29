# 01: Update VisualConfig default color palette and property tests

**What to build:** `VisualConfig.cs` exposes default hex colors matching `docs/palette.md` across all export groups (`Environment`, `Pins`, `Pockets`, `Cards & UI`, `Yakumono`, and `Ball Tiers`) with property names remaining strictly decoupled from specific palette names, verified by unit tests in `VisualConfigTests.cs`.

**Blocked by:** None (can start immediately)

**Status:** resolved

- [x] `VisualConfig.cs` exposes a new `[ExportGroup("Ball Tiers")]` with properties `BallTier1Color` (`#F3E8AA`), `BallTier2Color` (`#EAB879`), `BallTier3Color` (`#D1814C`), `BallTier4Color` (`#CA6642`), `BallTier5Color` (`#C04D38`).
- [x] `VisualConfig.cs` default values updated: `BackgroundColor` (`#1C261D`), `PinBaseColor` (`#B9CBD9`), `FlashColor` (`#F6E8A9`), `IndicatorBackgroundColor` (`#243026`), `IndicatorBorderColor` (`#304A31`), `ArmColor` (`#7B924E`), `CardBackgroundColor` (`#452A21`), `CardBorderColor` (`#D2814A`), `CardIndicatorBackgroundColor` (`#1C261D`), `YakumonoBaseColor` (`#CC6542`).
- [x] Property names in `VisualConfig.cs` remain strictly decoupled from color names (no palette names like `DarkestGreen` or `SlateBlue` as property names or public symbols).
- [x] Unit tests in `VisualConfigTests.cs` verify default property values match the intended palette and verify property setters emit the `Changed` signal.
- [x] Verification script (`.\scripts\verify.ps1` or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
