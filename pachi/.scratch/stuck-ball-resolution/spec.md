# Feature Spec: Stuck Ball Detection, Nudge, and Ball Refund System

Status: resolved

## Problem Statement

During gameplay, active balls falling through the level can become trapped in stationary equilibria or severe bottlenecks. This occurs either due to physics quirks (e.g. balls balancing directly atop pins or wedged between colliders) or intended physical congestion where high ball density produces traffic jams. When balls remain indefinitely stuck, the player is deprived of active balls, game progression stalls, and the player loses resources without any clear recourse.

## Solution

A robust, two-stage automatic resolution pipeline for stuck balls:
1. **Spatial Displacement Tracking**: While in active play, each ball continuously checks whether its physical position has left a localized radius within a detection time window. Micro-vibrations and physics jitter do not reset this timer.
2. **Physical Nudge Escalation**: If a ball stays stationary for the detection threshold (2.0 seconds), an upward and laterally randomized physical impulse (Nudge) is applied to dislodge the ball off pins or colliders. If still trapped after an additional interval (1.0 second), a second nudge impulse is applied.
3. **Ball Refund & Despawn**: If repeated nudges fail to free the ball after a total timeout (~4.5 seconds), the ball initiates a fade-out animation and despawns. Upon complete fade-out, an identical Ball Variant is re-queued into the Hopper without penalty.

## User Stories

1. As a player, I want balls that balance atop pins to automatically receive a physical nudge after a brief pause, so that they resume falling through the board without stalling my round.
2. As a player, I want jammed balls trapped in dense bottleneck clusters to be nudged, so that congestion is naturally relieved by physics impulses transferring through contacting balls.
3. As a player, I want a ball that cannot be dislodged after multiple nudges to automatically fade out and be refunded to my hopper, so that I never lose valuable ball variants to physics glitches.
4. As a player, I want refunded balls to appear in my hopper without penalty or loss of score, so that my game economy remains fair and uncorrupted by physics bugs.
5. As a player, I want balls waiting in the hopper queue or loaded in the launcher to be exempt from stuck detection, so that charging a shot or holding balls never triggers accidental nudges or despawns.
6. As a player, I want balls currently transitioning through pockets, drains, or fade animations to ignore stuck timers, so that normal game mechanics proceed uninterrupted.
7. As a game designer, I want nudge impulses, detection radii, and timeout durations to be inspector-configurable on the ball scene, so that physics parameters can be balanced and tuned easily.
8. As a game designer, I want balls to maintain independent detection timers, so that individual balls resolve their own jams without requiring complex central coordinator synchronization.
9. As a developer, I want stuck detection to measure spatial displacement rather than instantaneous velocity, so that physics engine sub-step micro-jitter does not reset the stuck timer.
10. As a developer, I want the ball refund to utilize existing global ball-awarding event buses, so that hopper dispensing and inventory management remain decoupled from ball physics.
11. As a developer, I want regression tests verifying displacement tracking, nudge invocation, and ball refund signals, so that future physics or scene changes do not break stuck ball resolution.

## Implementation Decisions

- **Domain Terminology**:
  - The stalled state is canonicalized as **Stuck Ball** (an active ball whose displacement remains within a minimum spatial threshold for longer than the detection window).
  - The physical dislodging impulse is canonicalized as a **Nudge** (an automatic upward and lateral impulse applied to dislodge a stuck ball).
  - The recovery despawn is canonicalized as a **Ball Refund** (the process that despawns a persistently stuck ball after failed nudges and re-queues an equivalent ball variant in the hopper).

- **Spatial Displacement Tracking Mechanism**:
  - The ball tracks an anchor position representing its last known stable coordinate.
  - In `_PhysicsProcess`, if the ball is active (unfrozen, not charging in the launcher, and in `TransitionState.None`), the distance between its current global position and anchor position is checked against a configurable displacement threshold (e.g. 10.0 pixels).
  - If distance exceeds the threshold, the anchor position updates to the current position and the elapsed stuck timer resets to zero.
  - If distance remains within the threshold, the elapsed stuck timer accumulates the delta time.

- **Nudge Escalation Pipeline**:
  - `Initial Nudge Window`: Configurable at 2.0 seconds. When reached, apply a vertical upward impulse with randomized horizontal deviation and increment the nudge counter.
  - `Retry Nudge Window`: Configurable interval (1.0 second per retry) up to a max retry count (2 nudges total).
  - `Impulse Calculation`: Impulse direction is directed primarily upward (negative Y in 2D space) with a randomized horizontal spread (e.g. angle range $[-30^\circ, 30^\circ]$ around Vector2.Up), scaled by a configurable impulse strength.

- **Ball Refund & Lifecycle Flow**:
  - `Refund Timeout`: Occurs when the stuck timer exceeds the max duration (~4.5 seconds total across initial window and retry intervals).
  - `Despawn & Refund Execution`:
    - Freeze physics interactions.
    - Initiate standard `FadeOut` visual animation.
    - Upon `FadeOutFinished`, emit global ball awarded event (`GlobalEvents.Instance.NotifyBallAwarded(Variant)`) to re-queue the identical `BallVariant` in the `Hopper`.
    - Free the stuck ball instance via `QueueFree()`.

- **Exemptions & Guardrails**:
  - Stuck detection is deactivated when `Freeze == true`.
  - Stuck detection is deactivated during active fade-in or fade-out transitions (`CurrentTransitionState != TransitionState.None`).
  - Stuck detection only accumulates when the ball has been launched into free play.

## Testing Decisions

- **Testing Philosophy**: Tests must exercise observable external behavior (physics state transitions, nudge impulses applied, timeout transitions, and event bus emissions) without relying on internal private field inspection.
- **Modules Tested**:
  - Ball physics lifecycle and stuck state transitions.
  - Displacement threshold reset behavior under movement.
  - Nudge impulse execution after initial timeout.
  - Ball refund event dispatch and node cleanup upon final timeout.
- **Prior Art**: Modeled after existing headless test suites in `src/art/tests/VisualConfigTests.cs` and executed via `TestRunner.cs`.

## Out of Scope

- Player-activated manual tilt/nudge buttons or gyroscopic board tilting.
- Dynamic visual UI warning indicators or alarms floating above stuck balls before nudging.
- Changing pin grid layouts or replacing level physics geometry.
- Modifications to score calculation for refunded balls (refunds simply replenish the hopper queue).

## Further Notes

- Because Godot physics bodies in resting equilibrium or micro-collision may generate slight floating-point velocity fluctuations, the displacement-based check ensures deterministic stuck detection across all frame rates and physics tick rates.
