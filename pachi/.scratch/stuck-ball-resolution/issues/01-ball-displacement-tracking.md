# Issue 01: Spatial Displacement Tracking for Stuck Balls

Status: resolved

## Description
Implement continuous spatial displacement tracking on `Ball.cs` to accurately detect when an active ball is trapped or stationary, while filtering out physics sub-step micro-vibrations and jitter.

## Acceptance Criteria
- [x] `Ball` tracks an anchor global position and accumulated stuck time.
- [x] When the ball moves farther than `StuckDisplacementThreshold` (e.g. 10.0 px), the anchor resets to the current position and the timer resets to 0.
- [x] Detection is active only when the ball is in active free play (`Freeze == false`, `CurrentTransitionState == TransitionState.None`).
- [x] Exported configuration properties for `StuckDisplacementThreshold` and `DetectStuck` enable clean inspector tuning.
