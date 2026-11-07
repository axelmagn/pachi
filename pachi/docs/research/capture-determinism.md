# Capture determinism measurements

For: whoever revisits ADR 0002. Lets them: repeat the measurements behind it.

Measured 2026-11-06 on `main_game.tscn`, 300 frames, `launch_one.json`,
seed 5, with `scripts/capture.sh`.

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
- Likely cause: per-process randomized hash ordering in Rapier2D's Rust code.
