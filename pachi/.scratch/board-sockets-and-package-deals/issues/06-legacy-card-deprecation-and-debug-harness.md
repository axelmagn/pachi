# 06: Legacy Card System Deprecation & Developer Debug Harness

**What to build:** Permanently remove obsolete freeform drag-and-drop controllers, card sidebars, and sub-property modifier archetypes (`CardDragController`, `CardSidebar`, `CardUI`, `CardGenerator`, 10 modifier archetypes, `CardData`, `CardGuardrailTests`), clean up call sites in `Pocket.cs`, `Hopper.cs`, and `main_game.tscn`, and implement the `SocketDebugHarness` interim debug overlay in development builds (`#if DEBUG` / `OS.IsDebugBuild()`) for live interactive socket hot-swapping.

**Blocked by:** 04: Atomic Hot-Swap Teardown & 1:1 Hopper Queue Evacuation, 05: 2x3 Modular Starter Board Layout in Level.tscn

**Status:** ready-for-agent

- [ ] Legacy card drag-and-drop controllers and UI components (`CardDragController`, `CardSidebar`, `CardUI`, `CardGenerator`) and their `.uid` sidecars are deleted.
- [ ] 10 legacy modifier archetypes, `CardData.cs`, and obsolete drag guardrail tests are removed.
- [ ] `Pocket.cs`, `Hopper.cs`, and `main_game.tscn` are cleaned of obsolete drag target registrations and assertions.
- [ ] `SocketDebugHarness` overlay is implemented under debug guards (`#if DEBUG` / `OS.IsDebugBuild()`), providing UI controls to hot-swap cards and test ball evacuations across all 6 sockets in live play.
- [ ] `./scripts/verify.sh` passes completely with 0 errors and 0 warnings.
