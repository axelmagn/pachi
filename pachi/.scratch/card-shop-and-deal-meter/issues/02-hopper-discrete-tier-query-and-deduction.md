# 02: Hopper Discrete Tier Inventory & FIFO Cost Deduction

**What to build:** Extend `Hopper.cs` with discrete ball tier queries (`GetTierCount(int tier)`), validation (`HasBallCost(int tier, int count)`), and front-to-back FIFO cost deduction (`DeductBallCost(int tier, int count)`), preserving the exact relative order of all other queued and contained balls.

**Blocked by:** 01: Deal Meter Engine & Scoring Speed Multipliers

**Status:** resolved

- [x] `Hopper.cs` provides `GetTierCount(int tier)` returning the total count of balls matching the specified tier (1–6) across contained and queued balls.
- [x] `Hopper.cs` provides `HasBallCost(int tier, int count)` returning `true` if `GetTierCount(tier) >= count`.
- [x] `Hopper.cs` provides `DeductBallCost(int tier, int count)` which scans from front to back (starting in `_containedBalls`, then `_queuedBalls`), removing the earliest `count` balls matching `tier`, freeing/disposing them, and returning `true` (or `false` if insufficient).
- [x] `Hopper.cs` emits a signal `InventoryChanged()` whenever balls are added, dispensed, awarded, or deducted.
- [x] Unit tests in `src/hopper/tests/HopperCostDeductionTests.cs` verifying tier counts, cost validation, front-to-back FIFO deduction order preservation, and failure on insufficient balls.
- [x] `./scripts/verify.sh` passes with 0 errors and 0 warnings.
