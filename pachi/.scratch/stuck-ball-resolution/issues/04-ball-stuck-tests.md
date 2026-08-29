# Issue 04: Headless Ball Stuck Test Suite

Status: resolved

## Description
Create a headless test suite (`src/balls/tests/BallStuckTests.cs`) and register it with `TestRunner.cs` to verify displacement tracking, nudge firing, movement resets, refund event emission, and lifecycle exemptions.

## Acceptance Criteria
- [x] Test that displacement tracking accumulates time when ball remains stationary and resets when ball moves > threshold.
- [x] Test that initial nudge impulse triggers at 2.0s and second nudge triggers at 3.0s.
- [x] Test that ball refund triggers at ~4.5s and invokes `GlobalEvents.Instance.NotifyBallAwarded(Variant)`.
- [x] Test that frozen balls and balls undergoing transitions ignore stuck timers.
- [x] Registered with `TestRunner.cs` and passes `./scripts/verify.sh`.
