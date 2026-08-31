# 03: Card Shop Engine, Deal Cursor & Discard Lifecycle

**What to build:** Implement `CardShop` engine in `src/cards/shop/CardShop.cs` managing the Master Deck draw pile, 3-row grid (3 cards per row), top-down Deal Cursor cycle (Row 0 -> Row 1 -> Row 2), deal overwrite on meter fill (discarding old cards in targeted row, drawing fresh cards), row discard on card purchase, and finite deck exhaustion state.

**Blocked by:** 01: Deal Meter Engine & Scoring Speed Multipliers, 02: Hopper Discrete Tier Inventory & FIFO Cost Deduction

**Status:** resolved

- [x] `CardShop` class created in `src/cards/shop/CardShop.cs` inheriting `Node` with `[GlobalClass]`.
- [x] Maintains `MasterDeck` (`List<PackageDealCard>`), `DiscardPile` (`List<PackageDealCard>`), and a 3x3 grid `PackageDealCard?[,]` (3 rows, 3 columns).
- [x] Cursor state `CurrentTargetRow` (int in 0..2).
- [x] `Initialize(IEnumerable<PackageDealCard> masterDeck)`: Populates Row 0 with 3 cards, sets `CurrentTargetRow` to 1, and initializes deck.
- [x] `DealNextRow()`:
  - If Master Deck has 0 cards, emits `DeckExhausted()` and halts.
  - Moves any existing cards in `CurrentTargetRow` to `DiscardPile`.
  - Draws up to 3 cards from `MasterDeck` and places them into `CurrentTargetRow`.
  - Advances cursor: `CurrentTargetRow = (CurrentTargetRow + 1) % 3`.
  - Emits `RowDealt(int row, PackageDealCard?[] cards)` and `CursorMoved(int targetRow)`.
- [x] `PurchaseCard(int row, int col)`:
  - Validates card existence and Hopper affordability.
  - Deducts cost from Hopper.
  - Moves the remaining cards in `row` to `DiscardPile`, clearing `row`.
  - Emits `CardPurchased(PackageDealCard card, int row, int col)` and `RowDiscarded(int row)`.
- [x] Unit tests in `src/cards/shop/tests/CardShopTests.cs` verifying initial deal to Row 0, cursor pointing to Row 1, meter fill deal overwrite & discard, purchase row discard, partial deals, and deck exhaustion.
- [x] `./scripts/verify.sh` passes with 0 errors and 0 warnings.
