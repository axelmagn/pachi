# 01: Prototype Systems Audit Against Vision

Type: research
Status: resolved
Blocked by:

## Question

What is the current mechanical state of the prototype across ball launching, board physics, pocket payouts, ball tiering, and card upgrades, and how does each system currently align or diverge from the four pillars in `docs/vision.md`?

## Answer

The prototype audit identified key divergence points against `docs/vision.md`:
1. **Pachinko Feel**: Physics and pin collision audio/visuals are strong, but the launcher uses a discrete 1.0s charge/auto-fire rather than continuous stream dynamics.
2. **Incremental Depth**: Only 1 shallow loop exists (direct pocket pip edits). `BallVariant.BasePrice` is unused, and there is no secondary currency, shop, or prestige progression.
3. **Deckbuilding Divergence**: No player deck, draw pile, or drafting exists. Free procedural cards are generated on every pocket payout into an 8-card FIFO sidebar, causing high-friction drag-and-drop micromanagement.
4. **Yakumono & Pockets**: Pockets and tulip arms work well; center yakumono opens all regular pocket arms on jackpot payout.

Detailed report: [research-01-prototype-audit.md](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/game-design-refinement/research-01-prototype-audit.md)
