# 05: Main Game Wiring, Socket Targeting & Debug Harness Replacement

**What to build:** Wire `DealMeter`, `CardShop`, and `CardShopUI` into `main_game.tscn`, connecting GlobalEvents scoring signals (Pocket hits -> +10%/+0.5x, Yakumono hits -> +35%/+2.0x), implementing interactive socket placement targeting (clicking an affordable card highlights matching sockets; clicking a socket executes the purchase & mount), and replacing the standalone `SocketDebugHarness` in the RightColumn.

**Blocked by:** 04: Card Shop UI & 3x3 Card Slots

**Status:** resolved

- [x] `main_game.tscn` instantiates `CardShop` and `CardShopUI` inside the `RightColumn` container (284px width).
- [x] Connects `GlobalEvents.BallEnteredPocket` / `GlobalEvents.YakumonoPaidOut` to `DealMeter` scoring boosts.
- [x] Connects `DealMeter.DealThresholdReached` to `CardShop.DealNextRow()`.
- [x] Interactive Socket Targeting:
  - When a card in `CardShopUI` is selected, `Level.cs` / `Socket2D` highlights all matching sockets on the board.
  - Clicking a highlighted socket executes `CardShop.PurchaseCard(row, col)` and mounts the card into the selected `Socket2D`.
  - Unmounting flushes trapped balls to Hopper; new card mounts atomically.
  - Clicking elsewhere or pressing Escape cancels targeting mode.
- [x] Replaces `SocketDebugHarness` in `main_game.tscn` while preserving debug hotkeys/utilities if needed.
- [x] Full end-to-end integration test in `src/cards/shop/tests/CardShopIntegrationTests.cs`.
- [x] `./scripts/verify.sh` passes with 0 errors and 0 warnings.
