# Issue 03: Ball Refund and Despawn Lifecycle

Status: resolved

## Description
Implement the stage-2 ball refund and recovery mechanism on `Ball.cs`. When a ball remains stuck after exhausting all nudge attempts, initiate despawn and award an identical `BallVariant` back into the `Hopper` via `GlobalEvents`.

## Acceptance Criteria
- [x] Refund triggers when accumulated stationary duration exceeds total timeout (~4.5s across initial window and retry intervals).
- [x] Initiates `FadeOut()` visual animation and freezes physics interactions.
- [x] Upon `FadeOutFinished`, emits `GlobalEvents.Instance.NotifyBallAwarded(Variant)` to re-queue the ball variant in the `Hopper`.
- [x] Cleans up the stuck ball node via `QueueFree()`.
