# 01: Socket2D Node Contract, Category Validation & Lifecycle Signals

**What to build:** Implement the core `Socket2D` node (`[Tool]`, `[GlobalClass]`) with category validation (`SocketCategory`), in-editor visual bounding box and label gizmos, automatic starter child adoption on `_Ready()`, the `ISocketComponent` lifecycle interface, and four lifecycle signals (`ComponentMounting`, `ComponentMounted`, `ComponentUnmounting`, `ComponentUnmounted`).

**Blocked by:** None (can start immediately)

**Status:** ready-for-agent

- [ ] `SocketCategory` enum is defined with categories `BeetlePocket`, `Spinner`, and `Yakumono`.
- [ ] `ISocketComponent` interface is defined exposing category, bounds, and lifecycle hooks (`OnMounted`, `OnUnmounting`, `FlushActiveBalls`).
- [ ] `Socket2D` node inherits `Node2D`, exports `Category`, `SocketId`, and `BoundsSize`, and exposes `CanMount` category validation.
- [ ] In-editor `_Draw()` rendering draws category-color-coded dashed bounding boxes and centered label text when running in editor tool mode.
- [ ] `Socket2D._Ready()` automatically detects and adopts any pre-existing `ISocketComponent` child as its initial `CurrentComponent` and invokes `OnMounted`.
- [ ] `Socket2D` defines and dispatches all four lifecycle signals: `ComponentMounting`, `ComponentMounted`, `ComponentUnmounting`, and `ComponentUnmounted`.
- [ ] Automated headless test suite `SocketLifecycleTests.cs` verifies starter adoption, category validation, signal sequence, and is registered in `TestRunner.cs`.
