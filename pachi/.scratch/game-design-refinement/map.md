## Destination

A lean, actionable MVP Design Spec (`docs/design/mvp-spec.md`) and Feature Process (`docs/design/process.md`) defining the minimal complete playable ruleset: core pachinko launch and payout loop, in-run card shop with slot-based board-building upgrades on a single board, prestige reset with card pack unlocks, and baseline economy numbers aligned with `docs/vision.md`.

## Notes

- Domain: Pachinko Mechanics, Incremental Game Loops, Minimalist Deckbuilding, Slot-Based Board Upgrades.
- Core Vision: Simple, satisfying, sensory-rich pachinko incremental deckbuilder on a short timeline (`docs/vision.md`).
- Skills to consult: `grilling`, `domain-modeling`.

## Decisions so far

- [01: Prototype Systems Audit Against Vision](issues/01-prototype-systems-audit.md): Audited prototype systems; identified lack of real deckbuilding/shop, unused ball prices, and discrete launch timing as primary divergences from vision.
- [02: Feature Iteration & Vision Reconciliation Process](issues/02-feature-iteration-process.md): Established 4-stage feature lifecycle, 4-pillar scorecard, 3P hypothesis formula, and tiered prototyping in `docs/design/process.md`.
- [03: Core Loop, Shop & Board-Building Card Model](issues/03-core-loop-and-board-building.md): Established balls as sole in-run currency, Yakumono Fever drafting, and designated sockets for beetle pockets, pin blocks, and spinners ([ADR 0003](docs/adr/0003-designated-board-sockets.md)).
- [04: Prestige Reset & Economy Balance Model](issues/04-prestige-and-economy-model.md): Established Prize Meter scaling, prestige resets with Prize Tokens, progression tree frontier drafting, and telemetry-based balance calibration ([ADR 0004](docs/adr/0004-prestige-progression-tree-frontier.md)).
- [05: MVP Design Spec Synthesis](issues/05-mvp-spec-synthesis.md): Synthesized core loop, designated sockets, Yakumono Fever mode, in-run card shop, Prize Meter prestige reset, initial catalog, and telemetry specifications into [`docs/design/mvp-spec.md`](../../docs/design/mvp-spec.md).


## Not yet specified

- **Tactile Feedback & Visual Polish Checklist**: Minimal sound, particle, and lighting cues needed to make the single board feel engaging.

## Out of scope

- Academic game design frameworks (formal MDA writeups, extended 3P thesis documentation beyond the lightweight process template).
- Freeform physics-based placement (all board building uses designated grid slots).
- Multi-board catalogs, campaign modes, or meta-narrative progression.
- Keyword-heavy combo engine or complex card interactions.
- Direct C# implementation during this design pass (this effort delivers design specs and decisions).
