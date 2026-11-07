# 0003. Keep `GameConfig` and `GlobalEvents` as scene-owned singletons, frozen

- Status: accepted
- Date: 2026-11-06

## Context

The fresh-project target allows no autoloads or global state without an ADR.
Pachi has two `static Instance` globals. Neither is an autoload: each is a
node in `main_game.tscn` that sets `Instance` in `_EnterTree` and clears it
in `_ExitTree`, asserting there's only one.

- `GameConfig` holds the ball scene, the ball tiers, and the gameplay RNG
  (`Rng`, seeded from `SeedOverride` by the test harnesses). Used by `Ball`,
  `Hopper`, `Launcher`, `Pocket`, `Yakumono`.
- `GlobalEvents` is an event bus with five signals (`BallAwarded`,
  `CentralPocketPaidOut`, `BallEnteredPocket`, `YakumonoStateChanged`,
  `YakumonoPaidOut`). It decouples pockets and yakumono from the hopper,
  which live in unrelated branches of the tree.

About 27 call sites depend on them. Removing them now would be a big-bang
refactor with no feature to justify it (playbook A4).

## Decision

Keep both, under these rules:

1. **No new globals.** Nothing else gets a `static Instance` or an autoload
   without a new ADR.
2. **Frozen surface.** Don't add fields to `GameConfig` or signals to
   `GlobalEvents`. New wiring uses exported node references or signals
   connected by the parent scene.
3. **Scene-owned lifetime stays.** They remain nodes in `main_game.tscn`, so
   tests can load a fresh scene and get fresh instances, and seeding works.
4. **Shrink when touched.** When a ticket reworks a feature that uses them,
   it moves that feature off them:
   - Gameplay rules that need randomness take a `System.Random` (or a seed)
     as a parameter, as `AutoFireSampler` does, and live in `core/`.
     `GameConfig.Rng` is then just where the scene creates it.
   - A `GlobalEvents` signal is replaced by a direct, past-tense signal on
     the emitting node, connected by `main_game.tscn` or the level.
   The beetle / pocket-input ticket (P11) is the first expected to do this
   for `Pocket`.
5. **Remove** each class once nothing references it.

## Consequences

- Scenes that use these globals can't run standalone; `AllScenesTests`
  already treats `hopper.tscn` and `launcher.tscn` as context-only.
- Global state stays visible in code review: any new `.Instance` usage is a
  rule-1/2 violation.
- No date on full removal: it follows feature work. Revisit if the globals
  still exist after the beetles cycle.
