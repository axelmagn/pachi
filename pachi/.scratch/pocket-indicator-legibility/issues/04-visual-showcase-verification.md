# 04: Visual Showcase Multi-Row Layout Integration and Verification

**What to build:** Complete visual demonstration in `visual_showcase.tscn` of 1–4 input and 1–8 output multi-row pocket indicators across test scenes, updating showcase test snapshots and verifying the complete test suite.

**Blocked by:** 02: Pocket and Yakumono Dynamic Stacking and Capacity Clamps, 03: Card Capacity Guardrails and CardUI Pip Alignment

**Status:** closed

- [x] `visual_showcase.tscn` pocket configurations display varied input (1–4) and output (1–8) ball counts.
- [x] Pockets and indicators render without overlap across 1-row and 2-row states in showcase.
- [x] Showcase screenshot test in `VisualConfigTests.cs` captures clean output.
- [x] `.\scripts\verify.ps1` (or `./scripts/verify.sh`) passes with 0 errors and 0 warnings.
