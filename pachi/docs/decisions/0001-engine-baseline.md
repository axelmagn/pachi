# 0001. Engine baseline: Godot 4.7.2 mono, Rapier2D, 960×540 pixel art

- Status: accepted
- Date: 2026-11-06

## Context

These settings were chosen during prototyping, before the playbook. This
ADR records them so they aren't changed by accident.

## Decision

- **Toolchain:** Godot `4.7.2.stable.mono`, `Godot.NET.Sdk/4.7.2`, .NET SDK 10.
- **Rendering:** Mobile renderer; 960×540, stretch mode `viewport`; pixel
  snap for 2D transforms and vertices; nearest-mipmap texture filter; pixel
  font with antialiasing, hinting, subpixel positioning and oversampling off.
- **Physics: Rapier2D**, chosen for performance with many balls at once.
  Every feel parameter (bounce, friction, launch force, nudge spread) is
  tuned against it. Not re-opened; its non-bit-exact replay is ADR 0002.
- **Physics layers:** 1 `Board` (every static body), 2 `Ball` (`ball.tscn`,
  mask `Board`+`Ball`; holes and pockets mask `Ball` only). A new layer is
  named in the commit that first uses it.
- **Input actions:** `launcher_charge` (Space). Code reads actions, not keys.
- **Main scene:** `src/main_game/main_game.tscn`, not renamed to the fresh
  layout's `src/main/main.tscn`.
- **Autoloads:** none of our own. The `godot_ai` plugin's
  `_mcp_game_helper` is stripped from every export by its
  `EditorExportPlugin` (verified 2026-11-06 on the Linux `.pck`).
- **Exports:** `Windows Desktop` and `Linux` (x86_64), both excluding
  `tests/*`; `Pachi.csproj`'s `ExportRelease` conditions drop test code.

## Consequences

- Art is authored for 960×540 at integer scale; sub-pixel tweens or
  non-integer zoom look wrong.
- A Rapier2D upgrade is a feel change and needs a playtest, not just a green
  gate.
- The inert `godot_ai` plugin files still ship in the pack; exclude
  `addons/godot_ai/*` if that ever matters.
- Changing renderer, resolution, or physics engine needs a superseding ADR.
