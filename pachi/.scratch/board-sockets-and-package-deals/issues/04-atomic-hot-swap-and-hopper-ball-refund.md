# 04: Atomic Hot-Swap Teardown & 1:1 Hopper Queue Evacuation

**What to build:** Implement the atomic synchronous hot-swap pipeline in `Socket2D`: validate category $\rightarrow$ emit `ComponentUnmounting` $\rightarrow$ flush trapped balls and append them 1:1 to the tail of the FIFO `Hopper` queue $\rightarrow$ disable `ProcessMode` and disarm descendant collision shapes $\rightarrow$ queue free outgoing instance $\rightarrow$ instantiate/mount incoming card scene $\rightarrow$ emit `ComponentMounted` $\rightarrow$ trigger detached audiovisual particle and latch feedback.

**Blocked by:** 02: Package-Deal Card Resource Model & Discrete Tier Ball Cost, 03: Component ISocketComponent Integration & Safe Ball Flush Protocol

**Status:** resolved

- [x] `Socket2D.MountFromCard(PackageDealCard card)` executes the strict synchronous teardown and instantiation sequence.
- [x] Active balls trapped or held inside the unmounting component are flushed 1:1 and appended to the tail of `Hopper._queuedBalls` via `Hopper.AddQueuedBalls`.
- [x] Outgoing component immediately has its `ProcessMode` set to `Disabled` and descendant `CollisionShape2D` shapes disabled before removal to eliminate physics ghosting.
- [x] Incoming component is instantiated from `card.ComponentScene`, parented to `Socket2D`, has local transform reset to zero, and receives `OnMounted`.
- [x] Lifecycle signals `ComponentUnmounting`, `ComponentUnmounted`, `ComponentMounting`, and `ComponentMounted` fire in exact sequence.
- [x] Visual particle burst and audio latch sound effect trigger upon successful component latching.
- [x] Headless tests in `SocketLifecycleTests.cs` verify hot-swap teardown, zero ball loss FIFO queue preservation, and collision shape deactivation.
