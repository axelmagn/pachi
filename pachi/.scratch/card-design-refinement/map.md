## Destination

A comprehensive standalone Card System Specification ([`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md)) defining the overhauled package-deal socket architecture, in-run shop deal meter and cursor lifecycle, discrete ball tier costs, and distinct Yakumono mechanics; synchronized with [`docs/design/mvp-spec.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md), updated domain terms in [`CONTEXT.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md), and relevant ADRs.

## Notes

- Domain: Pachinko Mechanics, Package-Deal Socket Components, Timed Shop Row Dealing, Discrete Tier Ball Economy, Yakumono Centerpiece Mechanics.
- Core Reference: [Brief](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/brief.md), [MVP Spec](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md), [CONTEXT.md](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md).
- Skills to consult: `grilling`, `domain-modeling`.

## Decisions so far

<!-- the index: one line per closed ticket, enough to judge relevance, then zoom the link for the detail the ticket holds -->

- [Package-Deal Socket Component Replacement Architecture](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/issues/01-package-deal-socket-architecture.md): Defined self-contained `PackedScene` component cards with strict 1:1 socket category matching, default starter socket population, and flush-and-refund cleanup on replacement.
- [Card Shop Meter, Row Cursor, and Discard Lifecycle](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/issues/02-shop-deal-meter-and-cursor-mechanics.md): Established 3x3 shop display, sequential top-down cursor cycle, 20s baseline passive meter with pocket (+10%, +0.5x) and Yakumono (+35%, +2x) boosts, hard clamp at 100%, row-discard on purchase/overwrite, and finite Master Deck exhaustion without mid-run reshuffle.
- [Discrete Ball Tier Cost and Hopper Economy Model](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/issues/03-discrete-tier-ball-cost-economy.md): Established strict FIFO hopper queue, front-to-back earliest match deduction for exact tier costs (1–4 balls), hopper-only inventory validation, unlimited queue capacity with HUD tier badges and head preview, and emergency drip zero-ball recovery.
- [Yakumono Card Archetypes and Fever Reward System](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/issues/04-yakumono-cards-and-fever-mechanics.md): Established 10s timed Fever with locked-open tulip wings, cumulative payout bursts and deal meter boosts on re-entry, and distinct centerpiece archetypes (Gatling Erupter, Alchemist Transmuter, Market Dynamo, Tulip Overdrive).
- [Card System Specification & MVP Design Sync](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/card-design-refinement/issues/05-card-system-spec-and-mvp-synthesis.md): Synthesized complete system design in [`docs/design/card-system.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md), recorded [ADR 0005](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md), synchronized [`docs/design/mvp-spec.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md), and updated domain terms in [`CONTEXT.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md).

## Not yet specified

- **Card Catalog Balance & Rarity Weights**: Concrete stat tuning and spawn weight curves across the full initial set of Master Deck cards.
- **Shop UI Layout & Cursor Polish**: Pixel-level visual layout and animation timing for 3-card rows and deal cursor transitions on mobile viewport.
- **Audio-Visual Kinetic FX Matrix**: Sound triggers, particle bursts, and tactile feedback for card dealing, discarding, and socket replacement.

## Out of scope

- Alternate launcher firing modes / manual tier selectors during runs (ruled out for MVP; strictly FIFO).
- Dynamic card fusion, merging, or in-run card crafting.
- Mid-run discard pile reshuffling or recycling (runs use finite deck exhaustion).
- Freeform physical positioning of board elements (all components mount strictly into designated sockets).
- Direct C# implementation during this design specification pass.
