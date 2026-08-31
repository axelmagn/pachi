# 01: Deal Meter Engine & Scoring Speed Multipliers

**What to build:** Implement `DealMeter` node/class managing passive 20.0s fill time ($5.0\%/\text{s}$), instant flat score chunks ($+10\%$ for pocket hits, $+35\%$ for Yakumono hits), temporary speed multiplier boosts ($+0.5\times$ and $+2.0\times$ lasting 5.0s), additive multiplier stacking, 100% hard clamp, threshold signal emission (`DealThresholdReached`), and reset.

**Status:** resolved

- [x] `DealMeter` class created in `src/cards/shop/DealMeter.cs` inheriting `Node` with `[GlobalClass]`.
- [x] Exports `BaselinePeriod` (default 20.0s), `PocketBoostChunk` (default 0.10f), `PocketSpeedMultiplier` (default 0.5f), `YakumonoBoostChunk` (default 0.35f), `YakumonoSpeedMultiplier` (default 2.0f), `BoostDuration` (default 5.0s).
- [x] Tracks current progress [0.0f, 1.0f], active speed boost timers, and net effective rate multiplier.
- [x] Connects or listens to scoring events (`AddPocketHit()`, `AddYakumonoHit()`).
- [x] Signals: `ProgressChanged(float progress, float effectiveRateMultiplier)`, `DealThresholdReached()`.
- [x] Methods: `Advance(double delta)`, `AddProgress(float amount)`, `AddSpeedMultiplier(float multiplier, float duration)`, `ResetProgress()`.
- [x] Unit tests in `src/cards/shop/tests/DealMeterTests.cs` verifying passive fill rate, flat boosts, temporary decaying speed multipliers, additive stacking, and threshold triggering.
- [x] `./scripts/verify.sh` passes with 0 errors and 0 warnings.
