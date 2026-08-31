# 05: Card System Specification & MVP Design Sync

Type: grilling
Status: resolved
Blocked by: 01, 02, 03, 04

## Question

How should the resolved package-deal socket model, shop deal meter loop, discrete tier economy, and Yakumono mechanics be synthesized into a standalone `docs/design/card-system.md` specification, synchronized with `docs/design/mvp-spec.md`, updated in `CONTEXT.md`, and recorded via ADR?

## Answer

1. **Standalone Card System Specification**:
   - Synthesized complete system design in [`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md) covering package-deal socket component replacement, 3x3 Card Shop layout and sequential top-down Deal Cursor cycle, passive and score-boosted Deal Meter pacing, discrete exact-tier ball costs ($1\text{--}4$ balls of Tier $1\text{--}4$), FIFO hopper queue, and distinct Yakumono Fever centerpieces.

2. **Architectural Decision Record**:
   - Authored [ADR 0005: Package-Deal Cards, Deal Meter Shop, and Discrete Tier Ball Economy](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md) documenting trade-offs against sub-property mutation, manual shop rerolls, and generic currency pricing.

3. **MVP Specification Synchronization**:
   - Updated [`docs/design/mvp-spec.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md) Section 4.2 (Yakumono Fever mechanics), Section 5 (Card Shop Deal Meter/Cursor loop), and Section 7 (discrete starter and card catalog pricing) to fully match the overhauled design.

4. **Domain Context Synchronization**:
   - Updated [`CONTEXT.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md) with canonical terms `Hopper Queue`, `Discrete Ball Cost`, `Emergency Drip`, and `Board Eruption`.

