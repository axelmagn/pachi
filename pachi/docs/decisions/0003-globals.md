# 0003. Keep `GameConfig` and `GlobalEvents` as scene-owned singletons, frozen

- Status: accepted
- Date: 2026-11-06

## Context

The fresh-project target allows no global state without an ADR. Pachi has
two `static Instance` globals, each a node in `main_game.tscn` that sets
`Instance` in `_EnterTree` and clears it in `_ExitTree`:

- `GameConfig`: ball scene, ball tiers, and the seeded gameplay RNG (`Rng`).
- `GlobalEvents`: a bus of five signals that decouples pockets and yakumono
  from the hopper, which live in unrelated branches of the tree.

About 27 call sites depend on them. Removing them now would be a big-bang
refactor with no feature to justify it (playbook A4).

## Decision

1. **No new globals** (`static Instance` or autoload) without a new ADR.
2. **Frozen surface.** No new fields on `GameConfig` or signals on
   `GlobalEvents`; new wiring uses exported refs or parent-connected signals.
3. **Scene-owned lifetime stays,** so tests get fresh instances and seeding
   works.
4. **Shrink when touched.** A ticket that reworks a feature using them moves
   it off: randomness-needing rules take a `System.Random` and live in
   `core/` (like `AutoFireSampler`); a `GlobalEvents` signal becomes a
   past-tense signal on the emitter, connected by the scene. P11 (beetle
   pocket input) is expected to do this first, for `Pocket`.
5. **Remove** each class once nothing references it.

## Consequences

- Scenes using them can't run standalone (`AllScenesTests` treats
  `hopper.tscn` and `launcher.tscn` as context-only).
- Any new `.Instance` usage in review is a rule 1/2 violation.
- Removal follows feature work; revisit if they outlive the beetles cycle.
