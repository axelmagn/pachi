# 0002. Captures are reproducible to a few pixels, not bit-exact

- Status: accepted
- Date: 2026-11-06

## Context

`scripts/capture.sh` records a scene at a fixed 60 fps with a fixed seed
(`GameConfig.SeedOverride` and `GD.Seed`) and a scripted input file. The
setup's determinism check asks whether two captures with the same seed and
input produce the same frames.

Measured on `main_game.tscn`, 300 frames, `launch_one.json`, seed 5:

- Different seeds, or input versus no input, diverge on 100+ frames, so the
  seed and the input both take effect.
- Wall-clock time was one source of drift: `MotionTrail2D` aged trail points
  with `Time.GetTicksMsec()`. It now uses accumulated physics delta.
- After that fix, repeated identical runs produce one of **two** outcomes
  (4 runs: 3 and 1). Frames match exactly until about frame 187. After that,
  about 10–25 pixels differ, all around the launched ball: a sub-pixel
  divergence in its path.
- Setting `physics/rapier/parallel/num_threads=1` doesn't change this
  (4 runs: 2 and 2).

The divergence is inside the Rapier2D extension. A likely cause is
per-process randomized hash ordering in the Rust code. Our code can't
control it, and switching physics engines is out of scope (see ADR 0001).

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
