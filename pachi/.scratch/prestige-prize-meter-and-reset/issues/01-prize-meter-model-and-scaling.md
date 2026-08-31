# 01: Prize Meter Model and Scaling Logic

**Type:** task  
**Status:** resolved  
**Blocked by:** none  

## Description
Implement `PrizeMeter` class in `src/prestige/PrizeMeter.cs` with:
- Score tracking from scoring events (`BallEnteredPocket`, `BallAwarded`, or direct `AddScore(float points)`).
- BaseTarget = 100.0f, scaling by `Mathf.Pow(1.50f, TokensEarnedInRun)`.
- Token awards, progress clamping/carryover.
- Signals: `ProgressChanged(float current, float target, float percent)`, `PrizeTokenAwarded(int totalTokens, int tokensInRun)`, `MeterReset`.
- Pure C# unit tests in `src/prestige/tests/PrizeMeterTests.cs`.
