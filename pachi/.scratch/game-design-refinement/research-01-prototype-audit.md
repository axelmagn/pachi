# Research Audit: Prototype Systems Audit Against Vision

**Date**: 2026-08-29  
**Ticket**: [01: Prototype Systems Audit Against Vision](file:///.scratch/game-design-refinement/issues/01-prototype-systems-audit.md)  
**Target Reference**: [`docs/vision.md`](file:///docs/vision.md), [`CONTEXT.md`](file:///CONTEXT.md)

---

## Executive Summary

An in-depth code and systems audit of `src/` was conducted to evaluate the mechanical baseline of Pachi against the four vision pillars defined in `docs/vision.md`.

The prototype contains a functional physics core (Rapier2D, ball bouncing with sparks/audio, tulip pocket arms, stuck-ball nudges, and center jackpot triggers). However, there is a fundamental divergence in the **Deckbuilding** and **Incremental** pillars:
1. **No Real Deckbuilder**: Cards are not drawn from a player deck or bought from a shop; instead, they are generated for free on every pocket hit into an 8-card FIFO sidebar queue.
2. **Missing Progression Systems**: Progression is limited to micro-editing pocket input/output pips. There is no prestige system, no secondary progression layer, and `BallVariant.BasePrice` is completely unused.
3. **Pacing / Interaction Friction**: The continuous influx of free micro-upgrade cards creates tedious drag-and-drop management that conflicts with the "simple, relaxing, zone-out" pillar.

---

## System-by-System Audit

### 1. Ball Launching & Board Physics

#### A. Launcher Mechanics
- **Files**: [`src/launcher/Launcher.cs`](file:///src/launcher/Launcher.cs), [`src/launcher/LauncherModeIndicator.cs`](file:///src/launcher/LauncherModeIndicator.cs), [`src/launcher/launcher.tscn`](file:///src/launcher/launcher.tscn)
- **Manual Firing**: Operated via `launcher_charge` (Spacebar). The launcher rotates from `_startRotation` to `_endRotation` over `ChargeTime` (1.0s) and snaps back on release over `ReleaseTime` (0.2s), launching the ball with `LinearVelocity = launchDirection * LaunchSpeed * evaluatedPower` (Speed = 1100.0f).
- **Auto-Firing**: Periodically charges and releases via `AutoFireInterval` (1.0s). Power is sampled from `AutoFireWeightCurve` using a shuffled bag of size 20 with a ±0.04 jitter.
- **Hopper Feed**: Balls are popped from `Hopper`, duplicated to `Level.BallsRoot`, and animated into the launch chute via `FadeIn()`. `IsLaunchPointClear()` enforces a 24px clearance radius.
- **Evaluation**:
  - *Pachinko Authenticity*: Traditional pachinko utilizes continuous dial tension controlling a high-frequency stream (~100 balls/minute or ~1.6 balls/sec) rather than discrete charge-and-release cycles.
  - *Friction*: The 24px clearance check and fade-in animations cause occasional stutter when balls linger near the chute.

#### B. Physics, Pins & Boundaries
- **Files**: [`src/balls/Ball.cs`](file:///src/balls/Ball.cs), [`src/pins/Pin.cs`](file:///src/pins/Pin.cs), [`src/pins/PinGrid.cs`](file:///src/pins/PinGrid.cs), [`src/pins/PinFunnel.cs`](file:///src/pins/PinFunnel.cs), [`src/levels/BoundaryRect.cs`](file:///src/levels/BoundaryRect.cs)
- **Physics Engine**: Godot Rapier2D (`addons/godot-rapier2d`).
- **Wall Following**: Custom `_IntegrateForces` intercepts contacts against `wall_material` with shallow angles (<15°) and redirects velocity tangentially, preserving speed (`WallFollowSpeedPreservation = 0.5f`) to prevent balls losing momentum on outer curved borders.
- **Pin Collisions & Juice**: Pin impacts trigger visual recoil (`RecoilDistance = 2.0px`), scale pulse (1.75x back-ease), yellow color flash (`FlashColor = Color(1.0, 0.85, 0.2)`), and directional `CpuParticles2D` sparks scaled to impact velocity.
- **Audio Feedback**: Three distinct impact channels (`PinBounceAudioPlayer`, `BallBounceAudioPlayer`, `WallBounceAudioPlayer`) with pitch scaling mapped to impact strength (0.9x to 1.3x).

#### C. Stuck Ball Handling & Refunds
- **Files**: [`src/balls/Ball.cs`](file:///src/balls/Ball.cs), [`src/balls/tests/BallStuckTests.cs`](file:///src/balls/tests/BallStuckTests.cs)
- **Mechanism**: If ball displacement from anchor is <10px for `InitialNudgeDuration` (2.0s), an upward physical impulse (`NudgeImpulseStrength = 300.0f`, ±30° spread) is applied. Retries up to 2 times every 1.0s.
- **Refund**: If still stuck after `RefundTimeout` (4.5s), the ball fades out, is destroyed, and triggers `GlobalEvents.NotifyBallAwarded(Variant)` to return an identical ball back to the hopper.

---

### 2. Economy & Ball Tiering

#### A. Currency & Ball Resource Loop
- **Files**: [`src/hopper/Hopper.cs`](file:///src/hopper/Hopper.cs), [`src/drain/Drain.cs`](file:///src/drain/Drain.cs), [`src/main_game/GlobalEvents.cs`](file:///src/main_game/GlobalEvents.cs)
- **Balls as Currency**: The hopper queue (`_containedBalls` + `_queuedBalls`) represents the player's sole currency and ammunition.
- **Loss Condition (Drain)**: Balls falling past pockets enter `Drain` (`src/drain/Drain.cs`) and are permanently despawned.
- **Gain Condition (Pockets)**: Balls entering pockets trigger payouts (`OutputBalls` list) which invoke `GlobalEvents.NotifyBallAwarded()`, depositing new balls into the hopper queue.
- **Evaluation**: The core resource loop adheres strictly to the rule that balls are both the ammunition and the currency.

#### B. Ball Variants & Tiers
- **Files**: [`src/balls/BallVariant.cs`](file:///src/balls/BallVariant.cs), [`src/balls/tiers/tier_1.tres`](file:///src/balls/tiers/tier_1.tres) through [`tier_6.tres`](file:///src/balls/tiers/tier_6.tres)
- **Tiers Defined**: 6 tiers with placeholder colors and base prices:
  - Tier 1: Cream/Yellow, `BasePrice = 2`
  - Tier 2: Peach/Tan, `BasePrice = 1` *(Data bug: Tier 2 price is lower than Tier 1)*
  - Tier 3: Orange/Brown, `BasePrice = 4`
  - Tier 4: Deep Orange, `BasePrice = 8`
  - Tier 5: Reddish Brown, `BasePrice = 16`
  - Tier 6: Ruby Red, `BasePrice = 32`
- **Critical Finding**: `BasePrice` is completely unused across the entire codebase. Pockets and cards only perform identity checks on `BallVariant` object references. There is no score conversion, monetary multiplier, or currency valuation attached to higher-tier balls.

---

### 3. Card & Upgrade System

#### A. Card Flow & Lifecycle
- **Files**: [`src/cards/CardGenerator.cs`](file:///src/cards/CardGenerator.cs), [`src/cards/CardSidebar.cs`](file:///src/cards/CardSidebar.cs), [`src/cards/CardDragController.cs`](file:///src/cards/CardDragController.cs), [`src/cards/CardData.cs`](file:///src/cards/CardData.cs)
- **Trigger**: Every time a ball enters ANY pocket (`GlobalEvents.BallEnteredPocket`), `CardSidebar.AddPlayableCard()` calls `CardGenerator.GeneratePlayableCard()`.
- **Card Queue**: Cards are placed in an 8-card sidebar container. When the 9th arrives, the oldest card at the bottom is discarded (FIFO).
- **Cost**: **0 (Free)**. Players drag cards onto pockets or hoppers without spending balls or currency.

#### B. Card Archetypes
- **Files**: [`src/cards/archetypes/`](file:///src/cards/archetypes/)
  1. `BallPackArchetype`: Drops 6–12 balls into the hopper.
  2. `IncrementInputTierArchetype`: Changes 1 pocket input requirement to a higher tier (awards bonus ball pack).
  3. `DecrementInputTierArchetype`: Changes 1 pocket input requirement to a lower tier.
  4. `IncrementOutputTierArchetype`: Upgrades 1 pocket output ball to a higher tier.
  5. `DecrementOutputTierArchetype`: Downgrades 1 pocket output ball to a lower tier (awards bonus ball pack).
  6. `AddInputBallArchetype`: Adds a required input ball to pocket (max 4, awards bonus pack).
  7. `RemoveInputBallArchetype`: Removes an input requirement from pocket (min 1).
  8. `AddOutputBallArchetype`: Adds an output ball to pocket payout (max 8).
  9. `RemoveOutputBallArchetype`: Removes an output ball from payout (min 1, awards bonus pack).

#### C. Strategic Analysis & Shortcomings
- **No Deckbuilding**: There is no deck construction, no drafting, no card acquisition shop, and no discard/draw pile cycle.
- **Friction vs. Flow**: Because cards are continuously generated on every pocket hit, the player is forced into constant micro-management of pocket recipe slots, dragging cards across the screen while balls are in flight.
- **Shallow Effects**: All cards manipulate micro-recipe pips (+1 pip, -1 pip, tier +1, tier -1) or give ball packs. There are no board-altering effects (e.g., bumper bounciness, multi-ball splitters, frenzy modes, gravity shifts, pin value multipliers).

---

### 4. Yakumono & Pockets

#### A. Pockets & Tulip Arms
- **Files**: [`src/pockets/Pocket.cs`](file:///src/pockets/Pocket.cs), [`src/pockets/PocketBallsIndicator.cs`](file:///src/pockets/PocketBallsIndicator.cs), [`src/pockets/pocket.tscn`](file:///src/pockets/pocket.tscn)
- **Tulip Wings**: Pockets have animated left/right `CharacterBody2D` arm colliders. When open (rotated 60°), they physically widen the catch area.
- **Recipe Matching**: Pockets require an exact set of input ball variants (up to 4). Matching balls are consumed; non-matching balls are ejected via `RejectHole`.
- **Sensory Feedback**: Musical pitch progression (`AcceptAudioStreams` / `SemitonesPerStep`) ascends as input slots fill up, followed by a fanfare on payout.

#### B. Center Yakumono
- **Files**: [`src/yakumono/Yakumono.cs`](file:///src/yakumono/Yakumono.cs), [`src/yakumono/yakumono.tscn`](file:///src/yakumono/yakumono.tscn)
- **Visual Gimmick**: Features 3 rendering layers (Frame, animated Face, Foreground). Entering balls randomize the face graphic.
- **Jackpot Trigger**: When the Center Yakumono pays out, it enters `JackpotState` and fires `CentralPocketPaidOut`, which triggers `OpenArms(5.0s)` on all regular pockets across the board.
- **Evaluation**: The center-jackpot-to-board-wide-tulip-opening mechanic is an authentic pachinko highlight. However, the Yakumono itself lacks mechanical variation (no roulette, mechanical chutes, physical diverters, or mini-games).

---

## Pillar Alignment Matrix

| Vision Pillar | Status | Core Strengths | Divergences & Deficits |
| :--- | :---: | :--- | :--- |
| **1. It's Pachinko** | **Moderate** | • Balls are both ammo and payout currency.<br>• Physical tulip arms open/close dynamically.<br>• Satisfying pin impacts (sparks, audio pitch, recoil).<br>• Center Yakumono payout opens all board tulips. | • Launcher operates on discrete charge/release rather than a fluid, high-speed ball stream.<br>• Board elements are static; lacks mechanical gimmicks, spinners, chimes, and frenzy/fever modes. |
| **2. It's Incremental** | **Low** | • Pockets can be upgraded to pay more and higher-tier balls. | • Only 1 shallow progression loop exists (pocket pip tweaking).<br>• `BasePrice` on BallVariants is unused in code.<br>• **Zero secondary / meta-progression systems**: No prestige loop, no persistent unlocks, no multipliers, no currency accumulation. |
| **3. It's a Deckbuilder** | **Critical Divergence** | • Card drag-and-drop controller and UI indicators exist. | • **No deckbuilding exists**: No deck, discard pile, draw pile, or card drafting.<br>• Cards are free procedural spam on pocket entry.<br>• No prestige shop to buy cards with prestige points.<br>• Card effects lack synergy or macro-strategy. |
| **4. It's Simple** | **Moderate** | • Clear visual pip representation for inputs/outputs.<br>• Legible single-screen board layout.<br>• Simple physics interactions. | • 8-card FIFO sidebar creates high-friction micro-management during ball flight, undermining the relaxing "zone-out" aesthetic. |

---

## Actionable Recommendations for GDD & Overhaul

1. **Implement Authentic Stream Launching**:
   - Transition from manual charge-and-release to an adjustable dial/handle launcher delivering a continuous rhythmic stream of balls.
2. **Build True Deckbuilding Architecture**:
   - Establish actual Deck, Hand, Discard, and Shop data structures.
   - Replace the FIFO sidebar with shop purchases and run-based card drafting.
3. **Establish 2-Layer Progression & Prestige**:
   - *Inner Loop*: Run-level ball accumulation, pocket upgrades, and board scoring.
   - *Outer Loop*: Prestige reset converting run score into Prestige Points to purchase new cards and permanent deck upgrades.
4. **Expand Card Design Space Beyond Pocket Pips**:
   - Introduce Board Modifiers (bouncy pins, magnetic fields, splitter pins), Economy Modifiers (interest, tier multipliers), and Gimmick Triggers (instant tulip open, fever mode).
5. **Harmonize Ball Tier Economics**:
   - Connect `BallVariant.BasePrice` to payout scoring and prestige point calculations, fixing the Tier 2 inverted price bug.
