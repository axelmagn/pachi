# 0002. Captures are reproducible to a few pixels, not bit-exact

- Status: accepted
- Date: 2026-11-06

## Context

`scripts/capture.sh` records a scene at a fixed 60 fps with a fixed seed
(`GameConfig.SeedOverride` and `GD.Seed`) and a scripted input file. Two
captures with the same seed and input match exactly until about frame 187,
then differ by 10–25 pixels around the launched ball. Seed and input do take
effect. One wall-clock source of drift (`MotionTrail2D`) was fixed; the rest
is inside the Rapier2D extension, persists with one physics thread, and our
code can't control it. Switching physics engines is out of scope (ADR 0001).
Measurements: [docs/research/capture-determinism.md](../research/capture-determinism.md).

## Decision

Treat captures as **reproducible to within a small tolerance**, not
bit-exact.

- Compare captures by outcome: ball positions within a few pixels, scores,
  wallet and pocket state. Don't compare frame hashes.
- Keep seeding everything we own. Any new randomness goes through
  `GameConfig.Rng` (gameplay) or Godot's global RNG (cosmetic).
- Keep game-time, not wall-clock, in anything that affects what's drawn.

## Consequences

- No golden-frame tests. Visual review stays a human (or Claude) looking at
  `out/capture/` frames.
- An outcome test that needs exact physics must run long enough to absorb
  the small divergence, or assert ranges.
- Re-check this when Rapier2D is upgraded. If it becomes bit-exact,
  supersede this ADR.
