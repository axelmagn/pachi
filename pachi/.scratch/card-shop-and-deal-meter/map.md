## Destination

Implement the complete in-run Card Shop & Deal Meter System ([`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md), [ADR 0005](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md)), featuring passive and scoring Deal Meter acceleration, 3x3 grid display with top-down Deal Cursor cycling, exact discrete ball tier deduction from Hopper Queue, and socket installation flow in `main_game.tscn`.

## Notes

- Domain: Card Shop, Deal Meter, Speed Multipliers, 3x3 Card Grid, Top-Down Deal Cursor, Discrete Ball Tier Costs, FIFO Hopper Deduction.
- References: [`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md), [`CONTEXT.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md), [`docs/adr/0005-package-deal-cards-and-deal-meter-shop.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md).

## Decisions so far

- [Deal Meter Engine & Scoring Speed Multipliers](issues/01-deal-meter-and-speed-boost-model.md): Implemented `DealMeter` supporting passive accumulation, flat scoring boosts, temporary decaying speed multipliers, and 100% threshold trigger.
- [Hopper Discrete Tier Inventory & FIFO Cost Deduction](issues/02-hopper-discrete-tier-query-and-deduction.md): Added `GetTierCount`, `HasBallCost`, and front-to-back FIFO `DeductBallCost` to `Hopper` along with `InventoryChanged` reactivity.
- [Card Shop Engine, Deal Cursor & Discard Lifecycle](issues/03-card-shop-engine-and-cursor-lifecycle.md): Implemented `CardShop` engine with 3x3 grid, top-down sequential deal cursor, deal overwrite with discard, row discard on purchase, and deck exhaustion.
- [Card Shop UI & 3x3 Card Slots](issues/04-card-shop-ui-and-card-item-slot.md): Implemented `CardShopUI` and `CardSlotUI` controls formatted for 284px right pane with deal meter progress, deck count, and cursor highlights.
- [Main Game Wiring, Socket Targeting & Debug Harness Replacement](issues/05-main-game-integration-and-socket-targeting.md): Wired `MainGameController` into `main_game.tscn`, connecting scoring boosts, interactive socket targeting and mounting, and replacing debug harness.

## Tickets

- [01: Deal Meter Engine & Scoring Speed Multipliers](issues/01-deal-meter-and-speed-boost-model.md) (resolved)
- [02: Hopper Discrete Tier Inventory & FIFO Cost Deduction](issues/02-hopper-discrete-tier-query-and-deduction.md) (resolved)
- [03: Card Shop Engine, Deal Cursor & Discard Lifecycle](issues/03-card-shop-engine-and-cursor-lifecycle.md) (resolved)
- [04: Card Shop UI & 3x3 Card Slots](issues/04-card-shop-ui-and-card-item-slot.md) (resolved)
- [05: Main Game Wiring, Socket Targeting & Debug Harness Replacement](issues/05-main-game-integration-and-socket-targeting.md) (resolved)
