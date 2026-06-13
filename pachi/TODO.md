# Pachi TODO

## 2026-06-10

Based on design direction captured in [brainstorm-20260610.md](docs/brainstorm-20260610.md).

### 1. Ball Tiers & Balancing
- [ ] Add `Tier` (int/enum) and `BasePrice` (float) properties to [Ball.cs](content/ball/Ball.cs).
- [ ] Modulate ball sprite colors dynamically based on their tier.
- [ ] Establish a color-coded palette for different tiers (e.g. Tier 1: White, Tier 2: Blue, Tier 3: Purple, etc.).

### 2. Jackpot Upgrades & Effects
- [ ] Refactor [Jackpot.cs](content/jackpot/Jackpot.cs) to support upgradeable payouts (ball count, ball tier).
- [ ] Create `JackpotCardEffect` resource base class inheriting from `CardEffect`.
- [ ] Implement `UpgradeJackpotCardEffect` (increases payout count or tier).
- [ ] Update [CardEffectControlReceiver.cs](content/card/CardEffectControlReceiver.cs) to accept `Jackpot` nodes as drag targets.

### 3. Card Shop & Deck System
- [ ] Add shop deck and discard pile configuration/management to [CardManager.cs](content/card/CardManager.cs).
- [ ] Implement deck shuffling, drawing, and card replenishment (`DealShopItems()`) in [CardShop.cs](content/card/CardShop.cs).
- [ ] Implement ball conversion card effects (e.g. convert 3 Tier 1 balls to 1 Tier 2 ball).

### 4. Currency System Overhaul (Hopper Balls as Currency)
- [ ] Replace `Cash` (abstract value) with hopper ball counts for card shop purchases.
- [ ] Update [Card.cs](content/card/Card.cs) and [CardShopItem.cs](content/card/CardShopItem.cs) to validate affordability based on the ball count in [Hopper.cs](content/hopper/Hopper.cs).
- [ ] Deduct card cost by destroying/removing the required number of balls from the hopper on purchase.
- [ ] Refactor or remove legacy Cash-based UI elements ([BuyBallsButton.cs](content/ui/BuyBallsButton.cs), [SellBallsButton.cs](content/ui/SellBallsButton.cs)).

### 5. Centerpiece Jackpot & Progression
- [ ] Add centerpiece jackpot flag/logic to [Jackpot.cs](content/jackpot/Jackpot.cs).
- [ ] Trigger shop deck card dealing and award Prestige XP when centerpiece jackpot is hit.
- [ ] Add Prestige XP and Prestige Points tracking to [Game.cs](content/game/Game.cs).
- [ ] Implement exponential scaling for Prestige XP thresholds (each level-up increases next threshold exponentially).

### 6. Prestige System & Board Resets
- [ ] Implement cooldown system after game start before prestiging is allowed.
- [ ] Implement `Prestige()` reset action in [Game.cs](content/game/Game.cs) (clears board, resets basic stats, awards prestige points).
- [ ] Create Prestige Shop UI/logic showing 3 randomly drawn "card packs" (Prestige Upgrades).
- [ ] Implement permanent upgrades: shop deck manipulation (add/remove cards), lever automation, and cooldown upgrades.

### 7. Unified Screen UI (ISEPS Style)
- [ ] Integrate the board, card shop, and prestige panels into a single continuous screen layout.
- [ ] Remove hard game phase pauses between play and shopping so play remains continuous.

## 2026-06-10 Legacy / Completed
- [x] Jackpots pay out to hopper
- [x] Hopper has max ball limit
- [x] Display countdown timer while playing
- [x] Game phase transitions between playing and shop
- [x] Card purchases & pricing
- [x] Card effects scaffolding
