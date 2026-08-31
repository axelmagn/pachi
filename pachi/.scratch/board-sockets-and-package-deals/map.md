## Destination

A complete, implementation-ready Technical Feature Specification ([`.scratch/board-sockets-and-package-deals/spec.md`](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/spec.md)) defining the typed `Socket2D` node architecture, `PackageDealCard` resource model, `ISocketComponent` lifecycle with safe flush-and-refund hot-swapping, starter board hierarchy in `Level.tscn`, and legacy drag-and-drop deprecation/test harness.

## Notes

- Domain: Pachinko Board Sockets, Package-Deal Component Swapping, Flush & Ball Refund Lifecycle, Starter Board Population, Godot 4.7 C# Scene Architecture.
- Core Reference: [Card System Spec](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/card-system.md), [MVP Spec](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/design/mvp-spec.md), [CONTEXT.md](file:///home/axel/workspace/axelmagn/pachi/pachi/CONTEXT.md), [ADR 0003](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0003-designated-board-sockets.md), [ADR 0005](file:///home/axel/workspace/axelmagn/pachi/pachi/docs/adr/0005-package-deal-cards-and-deal-meter-shop.md).
- Skills to consult: `grilling`, `domain-modeling`.

## Decisions so far

<!-- the index: one line per closed ticket, enough to judge relevance, then zoom the link for the detail the ticket holds -->

- [Socket Node Contract & Category Validation](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/01-socket-node-contract-and-category-validation.md): Defined typed Socket2D node with SocketCategory enum (BeetlePocket, PinBlock, Spinner, Yakumono), 4-stage lifecycle signals, and [Tool] editor bounding box preview.
- [Package-Deal Card Resource & Component Protocol](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/02-package-deal-card-resource-and-component-protocol.md): Defined PackageDealCard resource model with discrete tier costs (1–4 count, 1–4 tier) and ISocketComponent C# lifecycle interface for mounted scene roots.
- [Safe Flush & Hot-Swap Teardown Lifecycle](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/03-safe-flush-and-hot-swap-lifecycle.md): Defined atomic synchronous hot-swap lifecycle with 1:1 Hopper ball refunds, ProcessMode/collision suppression, and detached visual/audio latch feedback.
- [Starter Board Layout & Socket Hierarchy](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/04-starter-board-layout-and-socket-hierarchy.md): Defined 2x3 6-socket grid (1 Yakumono center, 2 Spinners with pins, 3 Beetle Pockets with pins) with embedded WYSIWYG starter scenes and consolidated 3-category enum.
- [Legacy Card System Migration & Test Harness](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/05-legacy-card-migration-and-test-harness.md): Defined complete legacy card drag-and-drop deletion plan, new automated SocketLifecycleTests and PackageDealCardTests suite, and interim SocketDebugHarness overlay.
- [Specification Synthesis & ADR Alignment](file:///home/axel/workspace/axelmagn/pachi/pachi/.scratch/board-sockets-and-package-deals/issues/06-spec-synthesis-and-adr-alignment.md): Synthesized findings from tickets 01–05 into the comprehensive technical specification at `.scratch/board-sockets-and-package-deals/spec.md`, reconciling with CONTEXT.md, ADR 0003, ADR 0005, and the Card System Spec.

## Not yet specified

- **Card Shop UI & Deal Meter Integration**: Full 3x3 shop display and Deal Cursor purchasing integration (handled in subsequent shop feature effort).
- **Dynamic Yakumono FX & Fever Particle Systems**: Audiovisual spectacle hooks during active component state transitions.

## Out of scope

- In-run freeform card drag-and-drop or modifying individual pin/pocket sub-properties (permanently replaced by Package-Deal Sockets).
- Mid-run discard pile reshuffling or recycling.
- Live C# production coding during this specification wayfinding effort.
