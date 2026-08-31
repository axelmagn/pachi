# Specification: Board Sockets & Package-Deal Cards

Status: resolved

## Problem Statement

In the original prototype, card upgrades operated via freeform drag-and-drop targeting individual board elements to apply incremental numeric or modifier mutations (such as +5% bounce or +1 payout). This approach created three severe game design and architectural problems:
1. **Broken Deflection Dynamics**: Freeform placement allowed players to cluster scoring pockets directly beneath the launcher entrance, bypassing obstacle pins and breaking the natural physical challenge of the pachinko board.
2. **Hidden State and Mutation Complexity**: Applying incremental modifiers directly to in-place nodes produced opaque state stacks, visual desynchronization, and complex undo/downgrade edge cases.
3. **Weak Tactile Upgrade Identity**: Minor stat nudges failed to deliver the satisfying, dramatic visual and mechanical transformations expected in a roguelike deckbuilder.

Players need a structured, reliable, and deeply tactile way to customize and upgrade their board without breaking the physics balance of the vertical playfield or accumulating hidden mutation state.

## Solution

We introduce **Board Sockets** and **Package-Deal Cards**:
1. **Designated Board Sockets**: The playfield features fixed, engineered mounting positions arranged in a balanced 2-row × 3-column modular grid (1 central Yakumono socket, 2 flanking Spinner sockets, and 3 lower Beetle Pocket sockets). Sockets enforce category constraints, preserving deflection lanes while allowing modular component variety.
2. **Package-Deal Scene Overwriting**: Each card contains a complete, self-contained component scene (including integrated pin deflectors, kinetic obstacles, and scoring mechanisms). Installing a card atomically flushes and frees the existing component instance and mounts the new archetype into the designated socket.
3. **Safe Flush & 1:1 Ball Refund Lifecycle**: Hot-swapping a component during active ball play safely evacuates any trapped or queued balls, converts them into their exact ball variants, and appends them to the tail of the FIFO Hopper Queue with zero ball loss.
4. **WYSIWYG Starter Hierarchy**: Level layouts come pre-populated with starter component child instances inside sockets, giving level designers and developers an accurate in-editor visual preview and enabling automatic runtime component adoption.
5. **Discrete Exact-Tier Ball Cost**: Cards require an exact count (1–4) of a specific ball tier (1–4) drawn from the Hopper Queue, tying card acquisition directly to player board performance and ball transmutation.
6. **Legacy Card System Deprecation**: All legacy freeform drag-and-drop controllers, card sidebars, and sub-property modifier archetypes are removed in favor of socket-driven package deals.

## User Stories

1. As a player, I want to upgrade my board by installing Package-Deal Cards into designated sockets, so that I can dramatically alter my board's scoring and kinetic mechanics.
2. As a player, I want each card to completely replace the previous socket component with an integrated scene, so that the board's appearance and behavior remain clean, predictable, and free of hidden modifier stacks.
3. As a player, I want sockets to strictly enforce category matching (Beetle Pocket, Spinner, Yakumono), so that I cannot place an incompatible component into the wrong board location.
4. As a player, I want any balls trapped or held inside a component to be safely refunded 1:1 into my Hopper Queue when I hot-swap that socket, so that I never lose resources due to upgrading mid-run.
5. As a player, I want newly refunded balls to be appended to the tail of my strict FIFO Hopper Queue, so that my queued launch order is predictably maintained.
6. As a player, I want card purchasing to require an exact count of an exact ball tier (Discrete Ball Cost), so that my scoring choices and ball transmutations feel economically meaningful.
7. As a player, I want to see and hear a distinct mechanical latch and visual pulse when a new component locks into a socket, so that the upgrade feels physically tangible and rewarding.
8. As a player, I want incoming balls to cleanly deflect off freshly mounted components without physics glitches, overlapping colliders, or phantom collisions from the removed component.
9. As a player, I want starter components to be present on the board from the moment a run begins, so that I have a fully functional pachinko board immediately without manual setup.
10. As a level designer, I want sockets to render clear bounding boxes and category labels in the Godot 2D editor viewport, so that I can arrange balanced socket layouts with accurate visual feedback.
11. As a level designer, I want to place starter component scenes directly as child nodes of sockets in the editor, so that I can visually compose and preview the complete board hierarchy WYSIWYG.
12. As a developer, I want `Socket2D` to automatically adopt any pre-existing child component on initialization, so that starter scenes configured in the editor transition seamlessly into active runtime components.
13. As a developer, I want `Socket2D` to expose clean lifecycle signals (`ComponentMounting`, `ComponentMounted`, `ComponentUnmounting`, `ComponentUnmounted`), so that UI, audiovisual systems, and game managers can react to component swaps without tight coupling.
14. As a developer, I want all socket components to implement a uniform `ISocketComponent` lifecycle contract, so that sockets can query category, bounds, unmount notifications, and ball flush callbacks agnostically.
15. As a developer, I want obsolete drag-and-drop controllers, card sidebars, and sub-property modifier archetypes removed, so that the codebase remains lean, type-safe, and focused on package-deal sockets.
16. As a developer, I want an interim debug harness overlay in development builds, so that I can interactively test hot-swapping package-deal cards across all sockets during live gameplay before the full shop UI is completed.
17. As a developer, I want automated headless test coverage for socket initialization, category enforcement, safe ball flushing, signal sequencing, and resource definitions, so that regressions are caught immediately in CI.

## Implementation Decisions

### 1. Spatial Socket Classification & Typed Architecture
- Sockets and package-deal cards share a consolidated 3-category spatial classification: `BeetlePocket`, `Spinner`, and `Yakumono`.
- Deflection pins are bundled directly inside component scenes as integrated obstacles rather than occupying isolated spatial sockets. Non-spatial passive relics are managed off-board by a dedicated manager.
- `Socket2D` is a custom 2D node (`Node2D`) registered with `[Tool]` and `[GlobalClass]` attributes. It exports `Category`, `SocketId` (a unique semantic string identifier such as `"yakumono_center"` or `"pocket_left"`), and `BoundsSize` for in-editor gizmo rendering.
- When running within the Godot editor, `Socket2D` renders a colored dashed bounding box and category label centered at its origin to assist level composition.

### 2. Component Contract & Resource Model
- Root nodes of all socketable scenes implement an `ISocketComponent` interface providing:
  - Component category identification.
  - Component spatial bounds.
  - Lifecycle hooks (`OnMounted` and `OnUnmounting`).
  - An evacuation callback (`FlushActiveBalls`) that extracts all currently trapped or queued ball variants.
- `PackageDealCard` is an exportable Godot `Resource` containing:
  - Identity and descriptive metadata (Card ID, Title, Description, Icon, Accent Color).
  - Target `SocketCategory`.
  - Target `PackedScene` component.
  - Discrete Ball Cost properties: `BallCostCount` (1–4) and `BallCostTier` (1–4).
  - Drafting weight metadata for shop generation.

### 3. Atomic Hot-Swap & Teardown Lifecycle
- Hot-swapping a component inside `Socket2D` executes synchronously in a strict sequential order:
  1. Validate that the card's category matches the socket's category and that the component scene reference is valid.
  2. Emit `ComponentUnmounting` signal and invoke `OnUnmounting` on the active component.
  3. Call `FlushActiveBalls` on the active component, passing an action that immediately appends refunded ball variants to the tail of the FIFO Hopper Queue.
  4. Disable the active component's processing mode (`ProcessMode.Disabled`) and disarm descendant collision shapes immediately to prevent physics ghosting.
  5. Detach the active component from the scene tree, schedule it for deletion (`QueueFree()`), and emit `ComponentUnmounted`.
  6. Instantiate the incoming card's `PackedScene`, attach it as a child of the socket, and reset its local transform (position to zero, rotation to zero).
  7. Emit `ComponentMounting`, invoke `OnMounted` on the incoming component, update the socket's active component reference, and emit `ComponentMounted`.
  8. Trigger detached audiovisual feedback (spawn visual particle burst at global position and play latch audio).

### 4. Starter Board Layout (`Level.tscn`)
- The 388 × 508 playfield is organized into a balanced 2-row × 3-column modular grid with 6 sockets:
  - Top row: `SocketSpinnerLeft` (-110, -40), `SocketYakumonoCenter` (0, -40), `SocketSpinnerRight` (110, -40).
  - Bottom row: `SocketPocketLeft` (-115, 130), `SocketPocketCenter` (0, 130), `SocketPocketRight` (115, 130).
- Sockets in `Level.tscn` contain starter component instances as pre-configured children. On `_Ready()`, `Socket2D` detects any existing `ISocketComponent` child, binds it as `CurrentComponent`, and calls `OnMounted` automatically without triggering unmount or refund lifecycles.

### 5. Legacy Card Migration & Clean Deletion
- All legacy drag-and-drop controllers (`CardDragController`), sidebar containers (`CardSidebar`), and individual card UI controllers (`CardUI`, `CardGenerator`) are deleted along with their `.uid` files.
- The 10 legacy modifier archetypes and `CardData` resource definitions are removed.
- `Pocket.cs` and `Hopper.cs` are cleaned of obsolete drag-and-drop references and assertions.
- `BallAwardIndicator` is retained and updated to render discrete bonus pip clusters for cards and pockets.

### 6. Developer Debug Harness
- An interim debug overlay (`SocketDebugHarness`) is included in development builds (`#if DEBUG` / `OS.IsDebugBuild()`).
- It presents a floating UI with mock package-deal cards and a socket selector, allowing developers and testers to hot-swap cards and test ball evacuations interactively during live gameplay.

## Testing Decisions

### What Makes a Good Test
Tests must evaluate public external behavior, state transitions, and contract invariants rather than private implementation details. A good socket test verifies that mounting a valid card replaces the active component, emits the correct signals in order, refunds trapped balls to the Hopper Queue, and disarms obsolete collisions without mutating internal properties.

### Modules Tested
1. **`SocketLifecycleTests`**:
   - Starter component auto-adoption in `_Ready()`.
   - Category validation (rejecting mismatched cards with clear failure state).
   - Safe flush and 1:1 ball refunding into the Hopper Queue.
   - Exact lifecycle signal emission sequence (`ComponentUnmounting` -> `ComponentUnmounted` -> `ComponentMounting` -> `ComponentMounted`).
   - Process mode and collision suppression during component teardown.
2. **`PackageDealCardTests`**:
   - Resource property constraints (discrete ball count 1–4, discrete ball tier 1–4, category matching).
   - Bonus ball variant generation and pip indicator data integrity.
3. **`PocketIndicatorTests`**:
   - Verification of pip rendering on modular pocket instances.

### Prior Art
Tests follow the established headless C# test runner pattern in `src/tests/TestRunner.cs` using the project's static assertion helper `TestAssert.cs`, matching the existing test suites in `src/balls/tests/BallStuckTests.cs` and `src/pockets/tests/PocketIndicatorTests.cs`.

## Out of Scope

- **Card Shop 3x3 UI & Deal Meter Integration**: The full 3-card row shop UI, Deal Meter timer accumulation, score boost acceleration (+10% for pockets, +35% for Yakumono), Deal Cursor row cycling, and Master Deck drafting are specified in ADR 0005 and will be implemented in a subsequent feature ticket.
- **Dynamic Yakumono FX & Fever Animations**: Advanced particle cascades, animated Board Eruption multi-ball ejections, and Yakumono transformation spectacles during Fever mode.
- **Mid-Run Discard Pile Reshuffling**: Master Decks remain finite per run without reshuffling or recycling.
- **Freeform Drag-and-Drop Card Placement**: Permanently deprecated in favor of designated socket package deals.

## Further Notes

- All C# scripts must adhere to `.editorconfig` (4 spaces, Allman braces, PascalCase types/methods) and compile cleanly with `dotnet build Pachi.sln` (`TreatWarningsAsErrors=true`).
- Godot `.cs.uid` sidecar files must be preserved for all newly created C# source files.
- The unified verification script `./scripts/verify.sh` must pass with 0 errors and 0 warnings before work is completed.
