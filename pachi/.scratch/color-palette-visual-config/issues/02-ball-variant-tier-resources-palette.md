# 02: Align Ball Variant tier resources with Ball Palette

**What to build:** All ball variant tier resources (`tier_1.tres` through `tier_6.tres` and `default_ball_variant.tres`) use `PlaceholderColor` values matching the Ball Palette defined in `docs/palette.md`.

**Blocked by:** #01: Update VisualConfig default color palette and property tests

**Status:** resolved

- [x] `src/balls/tiers/tier_1.tres` `PlaceholderColor` set to `#F3E8AA` (`ball-cream`).
- [x] `src/balls/tiers/tier_2.tres` `PlaceholderColor` set to `#EAB879` (`ball-peach`).
- [x] `src/balls/tiers/tier_3.tres` `PlaceholderColor` set to `#D1814C` (`ball-amber`).
- [x] `src/balls/tiers/tier_4.tres` `PlaceholderColor` set to `#CA6642` (`ball-orange`).
- [x] `src/balls/tiers/tier_5.tres` and `src/balls/tiers/tier_6.tres` `PlaceholderColor` set to `#C04D38` (`ball-red`).
- [x] `src/balls/default_ball_variant.tres` `PlaceholderColor` set to `#F3E8AA` (`ball-cream`).
- [x] Unit tests added or updated to assert ball variant tier resource `PlaceholderColor` matches expected palette values.
- [x] Verification script (`.\scripts\verify.ps1` or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
