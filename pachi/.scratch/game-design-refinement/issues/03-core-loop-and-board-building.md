# 03: Core Loop, Shop & Board-Building Card Model

Type: grilling
Status: resolved
Blocked by: 01

## Question

How should the active gameplay loop work across ball launching, pocket payouts, in-run card shop draws, and slot-based board building (placing and upgrading bumpers, pegs, and tulip pockets on designated slots of a single board) to provide simple, satisfying tactile gameplay?

## Answer

Established the core active gameplay loop, in-run card shop, and socket-based board building rules:

1. **Ball Launching & Currency Economy**:
   - Balls serve as the sole in-run resource for launching, purchasing shop cards, and manual rerolls.
   - Launching starts with a vintage charge-release manual flipper (rewarding precision launch velocities) and upgrades to continuous motorized streaming.
2. **Payouts & Fever State**:
   - Balls entering Beetle Pockets award ball payouts directly to the hopper.
   - Entering the centerpiece Yakumono triggers **Fever**: opens all pocket tulip wings, pays bonus balls, and deals 3 fresh cards to the in-run shop.
3. **Persistent In-Run Card Shop**:
   - 3 face-up cards drawn from the player's deck sit in a non-intrusive persistent sidebar.
   - Cards refresh on Yakumono Fever triggers or via manual ball-cost rerolls.
   - Cards are purchased with balls and apply immediately.
4. **Designated Board Sockets**:
   - Sockets on the single board are categorized into (1) Beetle Pocket Sockets, (2) Pin Block Sockets, and (3) Spinner Sockets, plus Global / Passive cards.
   - Prevents top-of-board pocket exploits while providing deep modular customization.
5. **Decisions & ADRs**:
   - Documented architectural decision in [ADR 0003](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0003-designated-board-sockets.md).
   - Added canonical domain terms (`Beetle Pocket`, `Socket`, `Pin Block`, `Fever`) to [CONTEXT.md](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md).

