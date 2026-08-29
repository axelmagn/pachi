# Issue 02: Physical Nudge Escalation Logic

Status: resolved

## Description
Implement the stage-1 physical nudge escalation pipeline on `Ball.cs`. When a ball is detected as stuck for the initial threshold duration, apply a physics impulse directed primarily upward with randomized lateral deviation to dislodge it off pins or colliders.

## Acceptance Criteria
- [x] First nudge fires after continuous stationary duration exceeds `InitialNudgeDuration` (default: 2.0s).
- [x] If the ball remains stationary, retry nudging every `NudgeRetryInterval` (default: 1.0s) up to `MaxNudgeRetries` (default: 2 nudges).
- [x] Nudge impulse applies an upward velocity / impulse with a randomized horizontal angle spread (`NudgeImpulseStrength`, `NudgeAngleSpreadDeg`).
- [x] Nudge count resets when the ball moves past `StuckDisplacementThreshold`.
