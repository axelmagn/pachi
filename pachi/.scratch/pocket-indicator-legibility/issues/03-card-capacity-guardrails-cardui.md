# 03: Card Capacity Guardrails and CardUI Pip Alignment

**What to build:** Target validation in `CardData.cs` preventing cards from exceeding pocket input (4) and output (8) capacity limits, and alignment of `CardUI.cs` pip sizing and spacing with the static 8x8 squircle dimensions.

**Blocked by:** 01: Static 8x8 Pip Grid and Multi-Row Indicator Rendering

**Status:** closed

- [x] `AddInputPocketCard.CanApply` returns `false` when target pocket or yakumono already has 4 input balls.
- [x] `AddOutputPocketCard.CanApply` returns `false` when target pocket or yakumono already has 8 output balls.
- [x] `CardUI.cs` renders static squircle pips consistent with the 8x8 dimensions and dynamic stroke outlines.
- [x] Unit tests validate card capacity guardrails for all relevant card types.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
