# 04: Card Shop UI & 3x3 Card Slots

**What to build:** Implement `CardShopUI` and `CardSlotUI` controls formatted to fit the 284px RightColumn pane in `main_game.tscn`. The UI displays the Deal Meter progress header with percentage and speed boost badges, deck count, 3 row containers with cursor target highlights, card slot cards showing title, cost, category, and affordability styling.

**Blocked by:** 03: Card Shop Engine, Deal Cursor & Discard Lifecycle

**Status:** resolved

- [x] `CardSlotUI` (`src/cards/shop/ui/CardSlotUI.cs` / `card_slot_ui.tscn`):
  - Displays Title, Category label, Ball Cost count and tier color badge (using `BallAwardIndicator` / tier dots), and Description.
  - Highlights whether the card is currently affordable from the Hopper.
  - Emits `CardSelected(int row, int col, PackageDealCard card)` on click.
- [x] `CardShopUI` (`src/cards/shop/ui/CardShopUI.cs` / `card_shop_ui.tscn`):
  - Fits within the `284px` width column.
  - Header: Deal Meter ProgressBar, Deal Progress %, Active Speed Multiplier badge (e.g. `1.5x`), Deck count badge (`Deck: 12`).
  - 3 Row Containers with 3 slots each.
  - Visual cursor indicator / border highlight identifying the active `CurrentTargetRow`.
  - Reacts to `DealMeter` progress updates, `CardShop` deal/purchase signals, and `Hopper.InventoryChanged`.
- [x] Unit tests in `src/cards/shop/tests/CardShopUITests.cs` verifying UI synchronization with engine state, cursor updates, and affordability state toggling.
- [x] `./scripts/verify.sh` passes with 0 errors and 0 warnings.
