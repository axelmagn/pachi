# 06: Legacy Card Deprecation, 3-Column Viewport Seam & Debug Harness

**What to build:** Permanently remove obsolete freeform drag-and-drop controllers, card sidebars, and sub-property modifier archetypes (`CardDragController`, `CardSidebar`, `CardUI`, `CardGenerator`, 10 modifier archetypes, `CardData`, `CardGuardrailTests`), clean up call sites in `Pocket.cs`, `Hopper.cs`, and `main_game.tscn`, establish the balanced 3-column screen viewport in `main_game.tscn` (reserving the 284px right-column vertical pane for the Card Shop), and implement the `SocketDebugHarness` interim debug overlay in development builds (`#if DEBUG` / `OS.IsDebugBuild()`) for live interactive socket hot-swapping.

**Blocked by:** 04: Atomic Hot-Swap Teardown & 1:1 Hopper Queue Evacuation, 05: 2x3 Modular Starter Board Layout in Level.tscn

**Status:** resolved

- [x] Legacy card drag-and-drop controllers and UI components (`CardDragController`, `CardSidebar`, `CardUI`, `CardGenerator`) and their `.uid` sidecars are deleted.
- [x] 10 legacy modifier archetypes, `CardData.cs`, and obsolete drag guardrail tests are removed.
- [x] `Pocket.cs`, `Hopper.cs`, and `main_game.tscn` are cleaned of obsolete drag target registrations and assertions.
- [x] `main_game.tscn` allocates a dedicated `RightColumn` container (`284px` width) in the 3-column layout, establishing the spatial Seam for the Card Shop vertical pane.
- [x] `SocketDebugHarness` overlay is implemented under debug guards (`#if DEBUG` / `OS.IsDebugBuild()`), providing UI controls to hot-swap cards and test ball evacuations across all 6 sockets in live play.
- [x] `./scripts/verify.sh` passes completely with 0 errors and 0 warnings.
