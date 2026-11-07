# 0001. Engine baseline: Godot 4.7.2 mono, Rapier2D, 960×540 pixel art

- Status: accepted
- Date: 2026-11-06

## Context

Pachi predates the setup playbook, so these settings were chosen during
prototyping, not decided up front. This ADR records them so they aren't
changed by accident, and says which ones are open.

## Decision

**Engine and toolchain.** Godot `4.7.2.stable.mono` (pinned in
`.godot-version`), `Godot.NET.Sdk/4.7.2`, .NET SDK 10 (`global.json`).

**Rendering.**

- Mobile renderer (`renderer/rendering_method="mobile"`).
- Viewport 960×540, stretch mode `viewport`.
- Pixel snap on for 2D transforms and vertices.
- Default canvas texture filter: nearest-mipmap (`default_texture_filter=3`).
- Pixel font with antialiasing, hinting, subpixel positioning, and
  oversampling off.

**Physics: Rapier2D** (`addons/godot-rapier2d`, `2d/physics_engine="Rapier2D"`).
It was chosen for more stable ball-on-pin stacking and contact than Godot
Physics 2D. Every feel parameter (bounce, friction, launch force, nudge
spread) is tuned against it, so switching would mean re-tuning all of them.
Not re-opened. Its non-bit-exact replay is accepted in ADR 0002.

**Physics layers.** All named in Project Settings:

| Layer | Name | Used by |
|---|---|---|
| 1 | `Board` | Pins, walls, yakumono: every static body (default layer) |
| 2 | `Ball` | `ball.tscn` (layer 2, mask `Board`+`Ball`); holes and pockets mask `Ball` only |

A new layer gets a name in the same commit that first uses it.

**Input actions.** `launcher_charge` (Space). Gameplay code reads actions,
never raw keys.

**Main scene.** `src/main_game/main_game.tscn`. The fresh-project layout
says `src/main/main.tscn`; we don't rename, it isn't worth the churn.

**Autoloads.** None of our own. `_mcp_game_helper` is added by the
`godot_ai` editor plugin. Its `EditorExportPlugin`
(`addons/godot_ai/export/mcp_export_plugin.gd`) strips the autoload from
every export. Verified 2026-11-06: the Linux release `.pck` has no
`_mcp_game_helper` entry and the build starts without autoload errors. No
feature tag or exclude filter needed.

**Export presets.** `Windows Desktop` and `Linux` (x86_64). Both exclude
`tests/*`; the `ExportRelease` conditions in `Pachi.csproj` keep test code
and test packages out of the assembly. Build with
`godot-mono --headless --export-release "Linux" ../build/linux/pachi.x86_64`.

## Consequences

- Art is authored for 960×540 at integer scale; anything that breaks
  pixel snap (sub-pixel tweens, non-integer zoom) looks wrong.
- Physics tuning is Rapier-specific. A Rapier2D upgrade is a feel change
  and needs a playtest, not just a green gate.
- The `godot_ai` plugin files still ship in the pack (inert without the
  autoload). Add `addons/godot_ai/*` to the exclude filters if pack size
  or cleanliness ever matters.
- Switching renderer, resolution, or physics engine needs a new ADR that
  supersedes this one.
