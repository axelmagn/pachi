# 03: Prestige Reset and Board Re-initialization Protocol

**Type:** task  
**Status:** resolved  
**Blocked by:** 01  

## Description
Implement reset methods across the system:
- `Hopper.ResetToStarterBalls(int count, BallVariant variant)`: Clears all contained and queued balls, cancels in-flight timers, and refills with standard starter balls.
- `Socket2D`: Support resetting / restoring starter component state.
- `Level.ResetAllSockets()` / `MainGameController.ExecutePrestigeReset()`:
  - Clears active balls in `BallsRoot`.
  - Resets all sockets to their starter package-deal components.
  - Resets `DealMeter` and deals fresh row in `CardShop`.
  - Resets `PrizeMeter` run progress and scaling level back to 0.
- Unit / integration tests in `src/prestige/tests/PrestigeResetTests.cs`.
