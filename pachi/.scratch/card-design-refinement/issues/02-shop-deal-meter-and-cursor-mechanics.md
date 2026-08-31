# 02: Card Shop Meter, Row Cursor, and Discard Lifecycle

Type: grilling
Status: resolved
Blocked by: 01

## Question

What are the exact timing rules, pacing values, score-based meter fill acceleration curves, cursor navigation behavior, 3-card row replacement/discard rules, and run lifecycle handling when the Master Deck is exhausted?

## Answer

1. **Shop Grid & Deal Cursor Cycle**:
   - The in-run Card Shop displays a fixed 3-row layout (3x3 grid, up to 9 cards displayed simultaneously).
   - The Deal Cursor advances strictly top-down in a sequential cycle: Row 0 &rarr; Row 1 &rarr; Row 2 &rarr; Row 0.
   - At run start, Row 0 is dealt immediately with 3 initial cards from the Master Deck; the Deal Meter starts at 0% with the Deal Cursor targeting Row 1.

2. **Deal Meter Pacing & Acceleration**:
   - **Baseline Passive Rate**: Fills in 20.0 seconds (5.0%/sec) under neutral play.
   - **Pocket Scoring Boost**: Instant flat chunk of +10% meter fill, plus a temporary +0.5x fill speed multiplier for 5.0 seconds.
   - **Yakumono Scoring Boost**: Instant flat chunk of +35% meter fill, plus a temporary +2.0x fill speed multiplier for 5.0 seconds.
   - **Deal Trigger & Hard Clamp**: When meter reaches 100%, the deal triggers immediately. The meter hard-clamps at 100% (no carryover of excess score burst percentage), resets to 0%, and active speed multipliers return to baseline (1.0x).

3. **Row Replacement & Discard Lifecycle**:
   - **Purchase Discard**: Purchasing 1 card from Row $N$ immediately moves the other 2 cards in Row $N$ to the Discard Pile, leaving Row $N$ empty until the Deal Cursor targets Row $N$ again.
   - **Deal Overwrite**: When the Deal Cursor triggers on Row $N$, any remaining cards currently occupying Row $N$ are sent to the Discard Pile, and up to 3 new cards are drawn from the Master Deck into Row $N$.
   - **Deal Cursor Advancement**: The Deal Cursor advances to `(CurrentRow + 1) % 3`.
   - **Atomic Concurrency Lock**: Initiating a card purchase locks that row; if the Deal Meter reaches 100% during socket selection/confirmation, the deal execution yields until the purchase completes (or aborts).

4. **Master Deck Exhaustion & Run Termination**:
   - Master Deck has a finite size per run (no in-run reshuffling from Discard Pile).
   - Partial deals (1 or 2 cards) occur if fewer than 3 cards remain in the Master Deck.
   - Once the Master Deck reaches 0 cards, the Deal Meter permanently halts and displays "Deck Exhausted".
   - Remaining dealt cards in the shop stay available for purchase until bought or until the run ends with a Prestige Reset.
