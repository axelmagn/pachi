# Feature Spec: Prize Meter & Prestige Reset Loop

Status: in-progress

## 1. Overview & Vision Alignment
Implements the core meta-progression driver from [MVP Design Spec Section 6](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md#L156-L196) and [ADR 0004](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0004-prestige-progression-tree-frontier.md).
Scoring balls in Beetle Pockets and the Yakumono advances the top-bar **Prize Meter**. When full, the player earns a **Prize Token**, and the target capacity scales exponentially by $1.50\times$. Holding $\ge 1$ Prize Token enables the **Prestige Reset** button, which wipes the board and hopper back to starter conditions while preserving meta tokens.

## 2. Core Mechanics & Formulas

### 2.1 Progress Tracking & Scoring
- **Scoring Event**:
  - Pocket catch / payout emits progress to the Prize Meter.
  - Formula: $\text{Progress} += \text{Ball Tier Value} \times \text{Pocket Multiplier}$
  - Tier Values: Tier 1 = 1, Tier 2 = 3, Tier 3 = 10, Tier 4 = 50, Tier 5 = 100, Tier 6 = 250 (as defined by `BallVariant.Value`).
  - Base target: $\text{BaseTarget} = 100$ points.
  - Capacity formula: $\text{Target}(L) = \text{BaseTarget} \times (1.50)^L$, where $L$ is the number of tokens earned in the current run.

### 2.2 Token Award & Level Scaling
- When $\text{CurrentProgress} \ge \text{TargetCapacity}$:
  - Increment Tokens (`TokensEarnedInRun++`, `TotalTokens++`).
  - Remaining progress is carried over or clamped: $\text{CurrentProgress} -= \text{TargetCapacity}$.
  - Target capacity scales to $\text{BaseTarget} \times (1.50)^L$.
  - Signal `PrizeTokenAwarded(int totalTokens, int tokensInRun)` is emitted.

### 2.3 Prestige Reset Lifecycle
- **Availability**: Enabled whenever `TotalTokens >= 1` (or `TokensInRun >= 1`).
- **Reset Execution**:
  - Flush and clear all airborne/in-flight balls on the board.
  - Clear Hopper contained balls and queued balls.
  - Refill Hopper with starter balls (e.g. 50 Tier-1 balls).
  - Reset all board sockets back to their starter package-deal components.
  - Reset DealMeter (0% progress) and deal a fresh starter row in CardShop.
  - Reset run Prize Meter progress ($0$) and level ($L = 0$, target $= 100$).
  - Emit `PrestigeResetCompleted` signal.

### 2.4 Top Bar UI
- Positioned in the top bar (`HBoxContainer2` in `main_game.tscn`).
- Visual elements:
  - **Prize Meter Progress Bar**: Smooth percentage fill, displaying `[Score / Target]` label.
  - **Prize Token Counter**: Displays `Tokens: X` badge with gold coin/token styling.
  - **Prestige Reset Button**: Disabled when 0 tokens, active & highlighted when $\ge 1$ tokens available.

## 3. Architecture & Seams
- `PrizeMeter` (`Node` in `src/prestige/PrizeMeter.cs`): Pure domain logic and signal bindings.
- `PrizeMeterUI` (`Control` in `src/prestige/ui/PrizeMeterUI.cs`): Presentation, progress bar, badges, and reset button.
- `Hopper.ResetToStarterBalls(int count, BallVariant variant)`: Clean hopper wipe and reload.
- `Socket2D.ResetToStarter()` / `Level.ResetAllSockets()`: Restoring default component hierarchy.
- `MainGameController`: Orchestration of reset and signal wiring.

## 4. Verification & Testing
- Unit tests for `PrizeMeter` (scaling formula, token incrementing, carryover, reset).
- Unit tests for `PrizeMeterUI` (visual updates, button enabled/disabled states).
- Integration test for full Prestige Reset loop.
- Verification via `./scripts/verify.sh`.
