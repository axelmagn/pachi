# Specification: Card Shop & Deal Meter System

Status: resolved

## Problem Statement

Players currently have designated board sockets and package-deal cards, but lack the in-run shop dealing loop to draft and purchase cards during active gameplay. The original prototype used static manual purchases and sub-property mutations; the new architecture requires a passive, score-accelerated Deal Meter, a top-down Deal Cursor across a 3x3 grid, exact discrete ball tier spending from the FIFO Hopper, and finite Master Deck exhaustion without mid-run reshuffling ([`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md), [ADR 0005](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md)).

## Solution

1. **Deal Meter Engine (`DealMeter`)**:
   - Manages passive timer accumulation ($20.0\text{s}$ baseline, $5.0\%/\text{s}$).
   - Processes instant flat boosts ($+10\%$ for pocket score, $+35\%$ for Yakumono score).
   - Manages decaying speed multipliers ($+0.5\times$ for pocket score, $+2.0\times$ for Yakumono score, each lasting $5.0\text{s}$). Multipliers stack additively: $\text{Rate} = 5\% \times (1.0 + \sum \text{boosts})$.
   - Hard clamps at $100\%$ and emits `DealThresholdReached` before resetting to $0\%$.

2. **Hopper Exact-Tier Validation & FIFO Deduction (`Hopper`)**:
   - `HasBallCost(int tier, int count)`: Checks if the hopper contains at least `count` balls matching the specified tier (1–4).
   - `DeductBallCost(int tier, int count)`: Scans front-to-back, removing the earliest `count` balls matching the target tier while preserving the relative ordering of all other queued and contained balls.
   - Emits an event / updates inventory when balls change.

3. **Card Shop Core Logic (`CardShop`)**:
   - Holds a finite Master Deck (`List<PackageDealCard>`) and Discard Pile.
   - Maintains a 3-row grid (each row holding up to 3 `PackageDealCard?` slots).
   - Top-down Deal Cursor cycling: `(CurrentRow + 1) % 3`.
   - On Meter Fill: Sends any remaining cards in the targeted row to the Discard Pile and deals up to 3 fresh cards from the Master Deck into that row.
   - On Card Purchase: Purchasing a card from Row $R$ discards the other cards in Row $R$, deducts the card's discrete ball cost from the Hopper, and returns the purchased card to be mounted into a matching socket.
   - Master Deck Exhaustion: When the Master Deck reaches 0 cards, dealing halts permanently ("Deck Exhausted"), leaving remaining dealt cards purchasable.

4. **Card Shop UI & Slot Views (`CardShopUI`, `CardSlotUI`)**:
   - Renders inside the $284\text{px}$ `RightColumn` container in `main_game.tscn`.
   - Header with Deal Meter progress bar, percentage text, active speed boost badge (e.g. `1.5x` / `3.0x`), and Deck Remaining count.
   - 3-row card container showing row labels (`ROW 1`, `ROW 2`, `ROW 3`) and deal cursor highlight on the active target row.
   - Card slot widget displaying Card Title, Category badge, Ball Cost indicator (using `BallAwardIndicator` / discrete tier pips), Description, and Purchase/Affordability button state.

5. **Socket Selection & Installation Flow**:
   - Clicking a purchasable card enters placement mode: eligible sockets matching the card's category highlight on the board.
   - Clicking a highlighted socket executes the purchase: deducts cost from Hopper, mounts the card into `Socket2D` (triggering flush-and-refund), and discards the rest of the shop row.

## Testing & Seams

- **Seam 1 (`DealMeter`)**: Test passive accumulation rate, instant boost addition, temporary speed multiplier addition, decay/expiration over time, 100% threshold signal emission, and reset.
- **Seam 2 (`Hopper`)**: Test discrete tier counting, insufficient ball rejection, front-to-back FIFO cost deduction preserving other balls, and empty queue edge cases.
- **Seam 3 (`CardShop`)**: Test initial 3-card deal to Row 0, cursor targeting Row 1, meter fill dealing into target row with old card discard, row discard on purchase, partial deals on deck depletion, and deck exhaustion halt.
- **Seam 4 (`CardShopUI` & Integration)**: Test affordability reactivity when hopper ball inventory changes, card purchase execution, socket mounting, and UI updates.
